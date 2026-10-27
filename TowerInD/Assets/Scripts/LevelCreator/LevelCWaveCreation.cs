//using JetBrains.Annotations;
//using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
//using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;

public partial class LevelC : MonoBehaviour
{

    // the actual Wave Controller
    [Header("WaveController")]
    private List<Waves> waveList;
    public WaveCreation wc;

    // the wave lists
    [Header("WaveControllerUI")]
    public GameObject waveUI;           // the main ui parent
    public List<GameObject> waveListsUI;
    public GameObject wavePrefab;
    public GameObject waveEnemyPrefab;
    public GameObject amtInput;         // the input field for amount
    public GameObject spawnInput;       // for spawnrate
    public GameObject waveInput;        // for waverate
    [Header("WaveUIDirectory")]
    public GameObject waveListParent;

    private GameObject selectedEnemy;   // selected enemy to add
    private Image selectedImage;

    [Header("ButtonTopUI")]
    public GameObject savePrefab;
    private GameObject currentSaveButton;

    // spawning
    private float spawnT;
    private float breakT;

    // setup
    public void SetupWaveControl()
    {
        waveListsUI = new List<GameObject>();
        waveListsUI.Add(Instantiate(wavePrefab, waveListParent.transform));

        // setup list
        waveList = new List<Waves>();
        waveList.Add(new Waves());
        waveList[0].enemies = new List<GameObject>();

        // start
        waveUI.SetActive(true);
        
        // setup save button
        currentSaveButton = Instantiate(savePrefab);
        currentSaveButton.transform.SetParent(topBar.transform, false);
        currentSaveButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ConfirmWaveSelection);
    }


    // get the current selection via enemy button press call
    public void SelectEnemy(GameObject button)
    {
        selectedEnemy = button.GetComponent<EnemyButton>().enemyPrefab;
        selectedImage = button.GetComponent<Image>();
    }


    // add to the current wave
    public void AddEnemy()
    {
        if (selectedEnemy == null || selectedImage == null)
            return;

        if (waveList == null || waveList.Count == 0)
            return;

        int waveNum = waveList.Count - 1;

        // get amount of enemies and put that many in
        for (int i = GetInput(amtInput); i > 0; i--)
        {
            // on first run setup ui portion
            if (i == GetInput(amtInput))
            {
                GameObject parent = waveListsUI[waveNum].transform.Find("EnemyData").gameObject;
                GameObject enemyIcon = Instantiate(waveEnemyPrefab, parent.transform);

                enemyIcon.GetComponent<Image>().sprite = selectedImage.sprite;
                enemyIcon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = i.ToString();
            }

            waveList[waveNum].enemies.Add(selectedEnemy);
        }
    }

    // remove the last entry
    public void CutEnemy()
    {
        if (waveList == null || waveList.Count == 0)
            return;

        int waveNum = waveList.Count - 1;

        if (waveList[waveNum].enemies == null || waveList[waveNum].enemies.Count == 0)
            return;

        GameObject enemyIcon = waveListsUI[waveNum].transform.Find("EnemyData").gameObject;
        enemyIcon = enemyIcon.transform.GetChild(enemyIcon.transform.childCount - 1).gameObject;

        int numberToCut = int.Parse(enemyIcon.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text);
        Destroy(enemyIcon);

        for (int i = numberToCut; i > 0; i--)
            waveList[waveNum].enemies.RemoveAt(waveList[waveNum].enemies.Count - 1);
    }

    // add a wave
    public void AddWave()
    {
        waveListsUI.Add(Instantiate(wavePrefab, waveListParent.transform));

        waveList.Add(new Waves());
        waveList[waveList.Count - 1].enemies = new List<GameObject>();
    }

    // remove a wave
    public void CutWave()
    {
        if (waveList == null || waveList.Count == 0 || waveListsUI == null || waveListsUI.Count == 0)
            return;

        int waveNum = waveList.Count - 1;

        //Destroy(waveListParent.transform.GetChild(waveListParent.transform.childCount - 1).gameObject);
        Destroy(waveListsUI[waveNum]);
        waveListsUI.RemoveAt(waveNum);
        waveList.RemoveAt(waveNum);
    }

    // getting the amount of enemies from the input field
    public int GetInput(GameObject inputT)
    {
        TMP_InputField input = inputT.GetComponent<TMP_InputField>();
        int.TryParse(input.text, out int amount);

        return amount;
    }
    public float GetInputF(GameObject inputT)
    {
        TMP_InputField input = inputT.GetComponent<TMP_InputField>();
        float.TryParse(input.text, out float amount);

        return amount;
    }

    // confirm the wave list and hide (For now actually call a save and save to json because we have no selectable units)
    public void ConfirmWaveSelection()
    {
        spawnT = GetInputF(spawnInput);
        breakT = GetInputF(waveInput);

        Debug.Log($"SpawnRate: {spawnT} \nWaveRate: {breakT}");

        if (spawnT <= 0 || breakT <= 0)
            return;

        waveUI.SetActive(false);
        Destroy(currentSaveButton);

        // temp save here
        SaveToJSON(null);

        // show test wave
        wc.EnemySpawn(waveList, spawnT, tracks, breakT);
    }

    
}
