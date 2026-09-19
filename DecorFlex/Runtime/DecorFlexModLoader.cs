using Engine;
using Engine.Graphics;
using Engine.Media;
using Engine.Serialization;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using TemplatesDatabase;
using System.IO;
using System.Text;
using XmlUtilities;
using Engine.Input;
using System.Globalization;

namespace Game
{
    public class DecorFlexModLoader : ModLoader
    {
        public SubsystemTerrain m_subsystemTerrain;
        public SubsystemFurnitureBlockBehavior m_subsystemFurnitureBlockBehavior;

        public static readonly string[] FurniturePackPaths = new string[]
        {
            "Assets/Furnitures/DecorFlex v1.0.0.scfpack"
        };

        public override void __ModInitialize()
        {
            ModsManager.RegisterHook("OnProjectLoaded", this);
            ModsManager.RegisterHook("OnLoadingFinished", this);
        }

        public override void OnProjectLoaded(Project project)
        {
            m_subsystemTerrain = project.FindSubsystem<SubsystemTerrain>(true);
            m_subsystemFurnitureBlockBehavior = project.FindSubsystem<SubsystemFurnitureBlockBehavior>(true);

            Time.QueueTimeDelayedExecution(Time.RealTime + 1.0, ImportFurniturePacksToWorld);
        }

        public void ImportFurniturePacksToWorld()
        {
            string logPrefix = "DecorFlexModLoader/ImportFurniturePacksToWorld";

            Log.Information($"[{logPrefix}] Checking and importing required furniture sets for this world...");

            try
            {
                // 1. Cập nhật danh sách pack CÓ THỂ import (trong thư mục manager)
                //   Mặc dù không trực tiếp dùng để kiểm tra tồn tại trong world, nhưng đảm bảo là pack file có sẵn để load nếu cần.
                FurniturePacksManager.UpdateFurniturePacksList();
                ReadOnlyList<string> availablePackFiles = FurniturePacksManager.FurniturePackNames;

                // 2. Lấy danh sách các FurnitureSet ĐÃ CÓ trong thế giới này
                //   Đây là thông tin quan trọng để kiểm tra
                ReadOnlyList<FurnitureSet> existingFurnitureSetsInWorld = m_subsystemFurnitureBlockBehavior.FurnitureSets;
                Log.Information($"[{logPrefix}] Found {existingFurnitureSetsInWorld.Count} furniture sets already loaded in this world.");

                int setsImportedThisSession = 0;
                int packsSkipped = 0;
                int packsFailed = 0;

                // 3. Lặp qua danh sách các pack cần đảm bảo có mặt
                foreach (string requiredPackFileName in GetFurniturePackFileNames())
                {
                    if (string.IsNullOrWhiteSpace(requiredPackFileName))
                    {
                        Log.Warning("[{logPrefix}] Skipping empty file name in FurniturePackPaths list.");
                        continue;
                    }

                    // 4. Kiểm tra xem THẾ GIỚI NÀY đã có FurnitureSet tương ứng chưa?
                    //   Chúng ta kiểm tra bằng trường ImportedFrom
                    bool setAlreadyInWorld = existingFurnitureSetsInWorld.Any(fs => string.Equals(fs.ImportedFrom, requiredPackFileName, StringComparison.OrdinalIgnoreCase));

                    if (setAlreadyInWorld)
                    {
                        Log.Information($"[{logPrefix}] FurnitureSet originating from '{requiredPackFileName}' already exists in this world. Skipping.");
                        packsSkipped++;
                        continue;
                    }

                    // --- Nếu bộ nội thất chưa có trong thế giới ---
                    Log.Information($"[{logPrefix}] FurnitureSet from '{requiredPackFileName}' not found in world. Attempting import...");

                    // 5. Kiểm tra xem file pack có tồn tại trong manager không để load
                    bool packFileAvailable = availablePackFiles.Contains(requiredPackFileName, StringComparer.OrdinalIgnoreCase);

                    if (!packFileAvailable)
                    {
                        Log.Error($"[{logPrefix}] Pack file '{requiredPackFileName}' needed for import is NOT AVAILABLE in FurniturePacksManager. Cannot import set.");
                        packsFailed++;
                        continue;
                    }

                    // 6. Bắt đầu quá trình import (giống logic của FurnitureInventoryPanel.ImportFurnitureSet)
                    try
                    {
                        List<FurnitureDesign> newlyAddedDesigns = new List<FurnitureDesign>(); // Lưu các design MỚI được thêm
                        int existingDesignCount = 0;
                        int failedDesignCount = 0;

                        // 6a. Load designs từ pack file
                        List<FurnitureDesign> designsFromPack = FurniturePacksManager.LoadFurniturePack(m_subsystemTerrain, requiredPackFileName);

                        if (designsFromPack == null || designsFromPack.Count == 0)
                        {
                            Log.Warning($"[{logPrefix}] Loaded 0 designs from '{requiredPackFileName}'. Cannot create set.");
                            packsFailed++;
                            continue;
                        }

                        // 6b. Lấy chuỗi designs và garbage collect
                        List<List<FurnitureDesign>> designChains = FurnitureDesign.ListChains(designsFromPack);
                        m_subsystemFurnitureBlockBehavior.GarbageCollectDesigns(); // Dọn dẹp trước khi thêm

                        // 6c. Thêm các designs vào subsystem behavior
                        foreach (List<FurnitureDesign> chain in designChains)
                        {
                            if (chain == null || chain.Count == 0) continue;

                            FurnitureDesign resultDesign = m_subsystemFurnitureBlockBehavior.TryAddDesignChain(chain[0], garbageCollectIfNeeded: false);

                            if (resultDesign == chain[0])
                            {
                                // Design MỚI được thêm thành công -> Lưu lại để link với Set
                                newlyAddedDesigns.Add(resultDesign);
                            }
                            else if (resultDesign != null)
                            {
                                // Design đã tồn tại từ trước (ví dụ từ pack khác import)
                                existingDesignCount++;
                            }
                            else
                            {
                                // Thêm design thất bại (hết chỗ?)
                                failedDesignCount++;
                            }
                        }
                        Log.Information($"[{logPrefix}] Design import result for '{requiredPackFileName}': New={newlyAddedDesigns.Count}, Existing={existingDesignCount}, Failed={failedDesignCount}.");

                        // 6d. TẠO FURNITURE SET MỚI và LIÊN KẾT designs (nếu có design mới được thêm)
                        if (newlyAddedDesigns.Count > 0)
                        {
                            // Lấy tên hiển thị (không có .scfpack)
                            string displayName = FurniturePacksManager.GetDisplayName(requiredPackFileName);

                            // Tạo Set mới, dùng tên pack làm nguồn gốc (ImportedFrom)
                            FurnitureSet newFurnitureSet = m_subsystemFurnitureBlockBehavior.NewFurnitureSet(displayName, requiredPackFileName);
                            Log.Information($"[{logPrefix}] Created new FurnitureSet '{newFurnitureSet.Name}' (from {requiredPackFileName})");

                            // Liên kết các design MỚI thêm vào Set này
                            foreach (FurnitureDesign newlyAddedDesign in newlyAddedDesigns)
                            {
                                m_subsystemFurnitureBlockBehavior.AddToFurnitureSet(newlyAddedDesign, newFurnitureSet);
                            }

                            Log.Information($"[{logPrefix}] Linked {newlyAddedDesigns.Count} new designs to the set '{newFurnitureSet.Name}'.");
                            setsImportedThisSession++;
                        }
                        else if (existingDesignCount > 0 && failedDesignCount == 0)
                        {
                            Log.Information($"[{logPrefix}] All designs from '{requiredPackFileName}' already existed. FurnitureSet was not created as no *new* designs were added.");
                            // Bạn có thể chọn tạo Set rỗng ở đây nếu muốn, nhưng thường không cần thiết
                        }
                        else
                        {
                            Log.Warning($"[{logPrefix}] No new designs were added from '{requiredPackFileName}'. FurnitureSet was not created.");
                        }

                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[{logPrefix}] Failed during import process for pack '{requiredPackFileName}'. Error: {ex.Message}");
                        packsFailed++;
                    }
                }

                Log.Information($"[{logPrefix}] Furniture check complete. Sets imported/created this session: {setsImportedThisSession}. Packs skipped (set existed): {packsSkipped}. Packs failed/unavailable: {packsFailed}.");

            }
            catch (Exception globalEx)
            {
                Log.Error($"[{logPrefix}] An unexpected error occurred during the furniture import check: {globalEx.Message}");
            }
        }

        public static string[] GetFurniturePackFileNames() => FurniturePackPaths.Select(Path.GetFileName).ToArray();

        public override void OnLoadingFinished(List<Action> actions)
        {
            actions.Add(() =>
            {
                ImportFurniturePacksToManager();
            });
        }

        public void ImportFurniturePacksToManager()
        {
            string logPrefix = "DecorFlexModLoader/ImportFurniturePacks";

            Log.Information($"[{logPrefix}] Starting furniture import check...");

            int requestedImports = 0;
            int skippedCount = 0;

            // Cập nhật danh sách gói nội thất để kiểm tra trùng
            FurniturePacksManager.UpdateFurniturePacksList();

            foreach (string relativePath in FurniturePackPaths)
            {
                if (string.IsNullOrWhiteSpace(relativePath))
                {
                    Log.Warning($"[{logPrefix}] Skipped empty or whitespace furniture path.");
                    skippedCount++;
                    continue;
                }

                try
                {
                    // Lấy tên tệp đầy đủ (ví dụ: "DecorFlex v1.0.0.scfpack")
                    string packFilename = Path.GetFileName(relativePath);
                    string currentPathForCallback = relativePath; // Biến cục bộ cho lambda

                    // Kiểm tra xem tên file có hợp lệ không
                    if (string.IsNullOrEmpty(packFilename))
                    {
                        Log.Warning($"[{logPrefix}] Could not derive valid filename from '{currentPathForCallback}'. Skipping.");
                        skippedCount++;
                        continue;
                    }

                    // Sử dụng StringComparer.OrdinalIgnoreCase để so sánh không phân biệt chữ hoa/thường
                    bool alreadyExists = FurniturePacksManager.FurniturePackNames.Contains(packFilename, StringComparer.OrdinalIgnoreCase);

                    if (alreadyExists)
                    {
                        Log.Information($"[{logPrefix}] Furniture pack '{packFilename}' already exists in FurniturePacksManager. Skipping import for {currentPathForCallback}.");
                        skippedCount++;
                        continue;
                    }

                    // Nếu chưa tồn tại, tiến hành yêu cầu tệp từ Entity
                    Log.Information($"[{logPrefix}] Pack '{packFilename}' not found. Requesting file: {currentPathForCallback}");
                    requestedImports++;

                    Entity.GetFile(currentPathForCallback, stream =>
                    {
                        // Mã bên trong callback khi GetFile thành công
                        try
                        {
                            if (stream == null)
                            {
                                Log.Warning($"[{logPrefix}] GetFile callback received a null stream for: {currentPathForCallback}. Skipping import.");
                                return;
                            }

                            Log.Information($"[{logPrefix}] Stream received for {currentPathForCallback}. Attempting to import as '{packFilename}'...");

                            // Gọi FurniturePacksManager để nhập (truyền trực tiếp tên packFilename với phần mở rộng đầy đủ)
                            string importedName = FurniturePacksManager.ImportFurniturePack(packFilename, stream);

                            // Nếu không có lỗi, ghi nhận thành công (có thể tên đã bị đổi nếu trùng)
                            Log.Information($"[{logPrefix}] Successfully imported '{packFilename}' as '{importedName}' from {currentPathForCallback}");

                            // Cập nhật lại danh sách ngay sau khi import thành công để lần check sau biết
                            // (Có thể không cần nếu bạn chỉ import 1 lần khi khởi động)
                            // FurniturePacksManager.UpdateFurniturePacksList();
                        }
                        catch (Exception importEx)
                        {
                            Log.Error($"[{logPrefix}] Failed to IMPORT furniture pack '{packFilename}' from {currentPathForCallback}. Error: {importEx.Message}");
                        }
                        finally
                        {
                            stream?.Dispose();
                            Log.Information($"[{logPrefix}] Stream disposed for {currentPathForCallback}");
                        }
                    }
                    /* , optional: Action<Exception> errorCallback */
                    // Xử lý lỗi GetFile nếu có
                    );
                }
                catch (Exception setupEx)
                {
                    Log.Error($"[OnLoadingFinishedLoader] Error SETTING UP import for {relativePath}. Error: {setupEx.Message}");
                    skippedCount++;
                }
            }

            Log.Information($"[{logPrefix}] Finished requesting furniture imports. Requested: {requestedImports}, Skipped (exists or error): {skippedCount}");
        }
    }
}
