using BepInEx;
using System;
using UnityEngine;

namespace MemorialBiography
{
    [BepInPlugin("cc.lymone.HoL.MemorialBiography", "MemorialBiography", "1.0.0")]
    public class MemorialBiography: BaseUnityPlugin
    {
        private void Start() {
            CopyMemberEventPanel();
        }
        public void CopyMemberEventPanel() {
            // 查找源对象和目标父对象
            GameObject sourceParent = GameObject.Find("AllUI/AllPanel/ZupuPanel/MemberEventPanel");
            Transform targetParent = GameObject.Find("AllUI/AllPanel/H_CiTang_Panel").transform;

            if (sourceParent == null || targetParent == null) {
                Debug.LogError("找不到指定的对象!");
                return;
            }

            // 复制对象
            GameObject copiedObject = Instantiate(sourceParent, targetParent);
            copiedObject.name = "MemberEventPanel"; // 移除"(Clone)"后缀

            //// 重置局部变换
            //copiedObject.transform.localPosition = Vector3.zero;
            //copiedObject.transform.localRotation = Quaternion.identity;
            //copiedObject.transform.localScale = Vector3.one;

        }
    }

}
