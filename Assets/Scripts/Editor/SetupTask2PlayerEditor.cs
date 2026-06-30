using UnityEngine;
using UnityEditor;
using SownInStone.Core;
using SownInStone.Storage;

namespace SownInStone.EditorTools
{
    public class SetupTask2PlayerEditor
    {
        [MenuItem("Tools/Auto Assign Task 2 References")]
        public static void AutoAssignTask2()
        {
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            if (player == null)
            {
                Debug.LogWarning("[TASK 2 SETUP] Không tìm thấy PlayerController trong Scene!");
                return;
            }

            Undo.RecordObject(player, "Auto Assign Task 2 References");

            player.floodBoardItem = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_flood_board.asset");
            player.sandbagItem = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_sandbag.asset");
            player.floodBoardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/FloodBoard.prefab");
            player.sandbagPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Sandbag.prefab");

            EditorUtility.SetDirty(player);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(player.gameObject.scene);

            Debug.Log($"[TASK 2 SETUP] Đã tự động gán đủ 4 tham chiếu Chắn lũ cho {player.gameObject.name} thành công!");
        }
    }
}
