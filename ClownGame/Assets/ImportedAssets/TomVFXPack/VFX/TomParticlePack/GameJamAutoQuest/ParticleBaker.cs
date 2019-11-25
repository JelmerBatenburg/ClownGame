using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBaker : MonoBehaviour
{
    public SkinnedMeshRenderer meshRender;
    public Mesh mesh;
    private bool updateMesh;
    public float updateDelay;

    public void UpdateMesh(bool shouldUpdateMesh)
    {
        updateMesh = shouldUpdateMesh;
        if (shouldUpdateMesh)
        {
            StartCoroutine(UpdateMeshRenderer());
        }
    }
    IEnumerator UpdateMeshRenderer()
    {
        while (updateMesh)
        {
            meshRender.BakeMesh(mesh);
            yield return new WaitForSeconds(updateDelay);

        }
    }
}
