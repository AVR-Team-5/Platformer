using UnityEditor;
using UnityEngine;

namespace Source.Utils
{
    public class AvatarMaker
    {
        [MenuItem("CustomTools/MakeAvatar")]
        private static void MakeAvatarMask()
        {
            var activeGameObject = Selection.activeGameObject;
 
            if (activeGameObject != null)
            {
                var avatar = AvatarBuilder.BuildGenericAvatar(activeGameObject, "");
                avatar.name = activeGameObject.name;
                Debug.Log(avatar.isHuman ? "is human" : "is generic");
 
                AssetDatabase.CreateAsset(avatar, "Assets/Avatars/" + avatar.name + ".asset");
            }
        }
    }
}