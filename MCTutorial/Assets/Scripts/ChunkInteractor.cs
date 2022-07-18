using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkInteractor : MonoBehaviour
{
    [SerializeField] private LayerMask ChunkInteractMask;
    [SerializeField] private LayerMask BoundCheckMask;
    [SerializeField] private Transform PlayerCamera;
    [SerializeField] private float InteractRange = 8f;
    private WorldGenerator WorldGenInstance;

    private void Start()
    {
        WorldGenInstance = FindObjectOfType<WorldGenerator>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray camRay = new Ray(PlayerCamera.position, PlayerCamera.forward);
            if (Physics.Raycast(camRay, out RaycastHit hitInfo, InteractRange, ChunkInteractMask))
            {
                Vector3 targetPoint = hitInfo.point - hitInfo.normal * .1f;

                Vector3Int targetBlock = new Vector3Int
                {
                    x = Mathf.RoundToInt(targetPoint.x),
                    y = Mathf.RoundToInt(targetPoint.y),
                    z = Mathf.RoundToInt(targetPoint.z)
                };

                string chunkName = hitInfo.collider.gameObject.name;
                if (chunkName.Contains("Chunk"))
                {
                    WorldGenInstance.SetBlock(targetBlock, 0);
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Ray camRay = new Ray(PlayerCamera.position, PlayerCamera.forward);
            if (Physics.Raycast(camRay, out RaycastHit hitInfo, 4f, ChunkInteractMask))
            {
                Vector3 targetPoint = hitInfo.point + hitInfo.normal * .1f;
                Vector3Int targetBlock = new Vector3Int
                {
                    x = Mathf.RoundToInt(targetPoint.x),
                    y = Mathf.RoundToInt(targetPoint.y),
                    z = Mathf.RoundToInt(targetPoint.z)
                };

                if (!Physics.CheckBox(targetBlock, Vector3.one * .5f, Quaternion.identity, BoundCheckMask))
                {
                    string chunkName = hitInfo.collider.gameObject.name;
                    if (chunkName.Contains("Chunk"))
                    {
                        WorldGenInstance.SetBlock(targetBlock, 2);
                    }
                }
            }
        }
    }
}
