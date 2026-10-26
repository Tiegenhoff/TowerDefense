using System.IO;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EditorButton : MonoBehaviour
{
    public GameObject editorUI;
    public GameObject selectionPrefab;
    public GameObject contentList;

    public void EditorButtonClick(bool open)
    {
        if (open)
        {
            // destroy existing
            foreach (Transform child in contentList.transform)
            {
                Destroy(child.gameObject);
            }

            // load each
            string[] saves = Directory.GetFiles(Application.persistentDataPath, "*.json");
            foreach (string s in saves)
            {
                GameObject selection = Instantiate(selectionPrefab, contentList.transform);
                TextMeshProUGUI selectionText = selection.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                selectionText.text = Path.GetFileName(s);
                // add the play button stuff here...
            }

        }

        editorUI.SetActive(open);
    }

}
