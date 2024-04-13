using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.TextCore.Text;
using WebSocketSharp;

public class Popup : MonoBehaviour
{
    public float lineHeight;
    public float imageHeight;
    public AudioClip Clip;

    public TextMeshPro Text;
    public Renderer Image;
    public Transform Backplate;
    public Transform Checkbox;

    public bool autoDestroy = false;

    void Start()
    {
        StartCoroutine(DestroyAfterSeconds(5f));
        Checkbox.gameObject.SetActive(false);
    }

    IEnumerator DestroyAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (autoDestroy)
        {
            Destroy(gameObject);
        }
    }

    public void WasMoved()
    {
        autoDestroy = false;
        Checkbox.gameObject.SetActive(true);
    }

    public void SetPopup(PopupData popupData_f)
    {
        Text.text = popupData_f.text;
        if (!popupData_f.image.IsNullOrEmpty())
        {
            Image.gameObject.SetActive(true);
            RenderImage(popupData_f.image);
        }
        else
        {
            Image.gameObject.SetActive(false);
        }

        IEnumerator _SetBackplate()
        {
            yield return new WaitForSeconds(0.1f);
            SetBackplateSize();
        }

        StartCoroutine(_SetBackplate());

        GetComponent<AudioSource>().PlayOneShot(Clip);
    }

    [ContextMenu("func SetBackplateSize")]
    public void SetBackplateSize()
    {
        float init_y = Backplate.localPosition.y;
        float init_height = Backplate.localScale.y;

        float height = 0.02f + (lineHeight * Text.textInfo.lineCount);

        if (Image.gameObject.activeSelf)
        {
            Image.gameObject.transform.localPosition = new Vector3(
                Image.gameObject.transform.localPosition.x,
                -0.05f - (lineHeight * Text.textInfo.lineCount),
                Image.gameObject.transform.localPosition.z);
            height += imageHeight + 0.1f;
        }

        if (Checkbox.gameObject.activeSelf)
        {
            Checkbox.transform.localPosition = new Vector3(
                Checkbox.transform.localPosition.x,
                -height + 0.14f,
                Checkbox.transform.localPosition.z
            );
            height += 0.02f;
        }
        height += 0.04f;

        float y = init_y - (height - init_height) / 2f;

        Backplate.localScale = new(Backplate.localScale.x, height, Backplate.localScale.z);
        Backplate.localPosition = new(Backplate.localPosition.x, y, Backplate.localPosition.z);

        // set box collider
        Vector3 quadSize = Backplate.GetComponent<MeshRenderer>().bounds.size;

        // Get the box collider
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.size = quadSize;
        }
    }

    [ContextMenu("func Debug_ShowData")]
    public void Debug_ShowData()
    {
        PopupData e = new();
        e.text = "this is an example popup";
        e.image = "";
        SetPopup(e);
    }

    private void RenderImage(string imageBase64)
    {
        // Check if the imageBase64 string is not empty
        if (!string.IsNullOrEmpty(imageBase64))
        {
            // Convert the Base64 string to a byte array
            byte[] imageData = System.Convert.FromBase64String(imageBase64);

            // Create a texture from the byte array
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(imageData);

            // Create a material and set the texture
            Material material = new Material(Shader.Find("Unlit/Texture"));
            material.mainTexture = texture;

            // Create a quad and apply the material
            Image.material = material;
        }
        else
        {
            Debug.LogWarning("Image Base64 string is empty or null.");
        }
    }
}
