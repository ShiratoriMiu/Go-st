using UnityEngine;
using System.Collections;

namespace Benjathemaker
{
    public class BuffItemAnim : MonoBehaviour
    {
        // ==== 回転関連 ====
        public bool isRotating = false;    // 回転アニメーションを有効にするか
        public bool rotateX = false;       // X軸回転するか
        public bool rotateY = false;       // Y軸回転するか
        public bool rotateZ = false;       // Z軸回転するか
        public float rotationSpeed = 90f;  // 回転スピード（角度/秒）

        void Update()
        {
            // === 回転処理 ===
            if (isRotating)
            {
                // 各軸のフラグに応じて回転方向ベクトルを作成
                Vector3 rotationVector = new Vector3(
                    rotateX ? 1 : 0,
                    rotateY ? 1 : 0,
                    rotateZ ? 1 : 0
                );

                // 1秒あたり rotationSpeed 度回転させる
                transform.Rotate(rotationVector * rotationSpeed * Time.deltaTime);
            }
        }
    }
}
