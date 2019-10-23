using UnityEngine;
public class IsVisible : MonoBehaviour {
    Renderer m_Renderer;
    Chunk chunk;
    Camera cam;
    // Use this for initialization
    void Start() {
        m_Renderer = GetComponent<Renderer>();
        chunk = GetComponentInParent<Chunk>();
        cam = chunk.player.GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update() {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        if(GeometryUtility.TestPlanesAABB(planes, m_Renderer.bounds)) {
            chunk.ChunckVisible();
        }
        /*if (m_Renderer.isVisible) {
            chunk.ChunckVisible();
        }*/
    }

}