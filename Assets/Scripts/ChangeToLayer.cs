using UnityEngine;

public class ChangeToLayer : MonoBehaviour
{
    private Camera m_MainCamera;
    [SerializeField]
    private string m_NewLayerMaskName = "Outlined";

    private void Start()
    {
        m_MainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);

                // If the impacted GameObject is already in the "Outlined" layer, then it's moved to the Default layer (0)
                if (layerName.Equals(m_NewLayerMaskName))
                {
                    hit.collider.gameObject.layer = 0;
                }
                // If the GameObject doesn't belong to the "Outlined", then it's moved to that layer in order to make work the OverrideMaterialRenderPassFeature
                else
                {
                    hit.collider.gameObject.layer = LayerMask.NameToLayer(m_NewLayerMaskName);
                }
            }
        }
    }
}
