using UnityEngine;

public class CameraEdgeIntersections : MonoBehaviour
{
    public Camera mainCamera; // 使用するカメラ
    public LayerMask layerMask; // チェックするオブジェクトのレイヤー

    Vector3[] limitPos = new Vector3[4];

    private void Start()
    {
        // ビューポートの上下左右の座標を定義
        Vector3[] viewportPoints = new Vector3[]
        {
            new Vector3(0.5f, 1f, mainCamera.nearClipPlane), // 上
            new Vector3(0.5f, 0f, mainCamera.nearClipPlane), // 下
            new Vector3(0f, 0.5f, mainCamera.nearClipPlane), // 左
            new Vector3(1f, 0.5f, mainCamera.nearClipPlane)  // 右
        };

        string[] directions = { "上", "下", "左", "右" };

        for (int i = 0; i < viewportPoints.Length; i++)
        {
            // ビューポート座標をワールド座標に変換
            Vector3 edgePoint = mainCamera.ViewportToWorldPoint(viewportPoints[i]);

            // カメラ位置からビューポート座標に向けてレイを作成
            Vector3 direction = (edgePoint - mainCamera.transform.position).normalized;
            Ray ray = new Ray(mainCamera.transform.position, direction);

            // レイキャストで交点を取得
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                //Debug.DrawLine(ray.origin, hit.point, Color.red); // デバッグ用に可視化
                Debug.Log($"方向: {directions[i]} 交点: {hit.point}, オブジェクト: {hit.collider.name}");
                limitPos[i] = hit.point;
            }
            else
            {
                //Debug.DrawLine(ray.origin, ray.origin + direction * 100f, Color.blue); // レイが当たらない場合のデバッグ
                Debug.Log($"方向: {directions[i]} オブジェクトにヒットしませんでした");
            }
        }
    }

    void Update()
    {
        
    }
}
