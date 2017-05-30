using UnityEngine;
using System.Collections;

public class Flash : MonoBehaviour {

    public Renderer[] meshes;
    bool flashing;


    // Use this for initialization
    void Start() {
        flashing = false;
    }

    // Update is called once per frame
    void Update() {

    }

    public void FlashMeshes(Color flashColor, float timeLight, float timeDark, int numFlashes)
    {
        if (flashing)
            StopCoroutine("FlashMesh");

        foreach (Renderer r in meshes)
        {
            StartCoroutine(FlashMesh(r, flashColor, timeLight, timeDark, numFlashes));
        }
    }

    public void FlashMeshes(Color flashColor, Color emission, float timeLight, float timeDark, int numFlashes)
    {
        if (flashing)
            StopCoroutine("FlashMesh");

        foreach (Renderer r in meshes)
        {
            StartCoroutine(FlashMesh(r, flashColor, emission, timeLight, timeDark, numFlashes));
        }
    }

    IEnumerator FlashMesh(Renderer mesh, Color color, float timeLight, float timeDark, int numFlashes)
    {
        flashing = true;
        Color originalColor = mesh.material.color;

        for (int i = 0; i < numFlashes; i++)
        {
            mesh.material.color = color;
            yield return new WaitForSeconds(timeLight);

            mesh.material.color = originalColor;
            yield return new WaitForSeconds(timeDark);

        }
        mesh.material.color = originalColor;
        flashing = false;
    }

    IEnumerator FlashMesh(Renderer mesh, Color color, Color emissionColor, float timeLight, float timeDark, int numFlashes)
    {
        flashing = true;
        Color originalColor = mesh.material.color;
        Color originalEmission = mesh.material.GetColor("_EmissionColor");

        Color eColor = emissionColor * originalEmission.a;

        for (int i = 0; i < numFlashes; i++)
        {
            mesh.material.color = color;
            mesh.material.SetColor("_EmissionColor", eColor);
            yield return new WaitForSeconds(timeLight);

            mesh.material.color = originalColor;
            mesh.material.SetColor("_EmissionColor", originalEmission);
            yield return new WaitForSeconds(timeDark);

        }
        mesh.material.color = originalColor;
        flashing = false;
    }

    bool isPaused = false;
    IEnumerator FlashMeshPause(Renderer mesh, Color color, Color emissionColor)
    {
        flashing = true;
        Color originalColor = mesh.material.color;
        Color originalEmission = mesh.material.GetColor("_EmissionColor");

        Color eColor = emissionColor * originalEmission.a;

        mesh.material.color = color;
        mesh.material.SetColor("_EmissionColor", eColor);

        isPaused = true;
        while (isPaused)
            yield return null;


        mesh.material.color = originalColor;
        flashing = false;
    }

    public void FlashPause(Color color, Color emissionColor)
    {
        if (!flashing)
        {
            foreach (Renderer r in meshes)
                StartCoroutine(FlashMeshPause(r, color, emissionColor));
        }
    }

    public void FlashUnPause()
    {
        isPaused = false;
    }
}
