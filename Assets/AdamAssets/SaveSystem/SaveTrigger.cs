using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveTrigger : MonoBehaviour , ISaveLoad
{

    public Vector3 Size;
    public LayerMask PlayerLayer;
    public Color GizmosColor;

    [SerializeField] private string _id; 
    public string ID { get { return _id; } }


    // Quest sistemi bu flagi true yaparak save triggeri aktiflestirebilir.
    public bool IsTriggerReady = false;



    Coroutine _saveCoroutine;
    void OnEnable() {
        if (PlayerLayer == default(LayerMask)) PlayerLayer = 1 << LayerMask.NameToLayer("Player");
        _saveCoroutine = StartCoroutine(nameof(SaveCheckCoroutine)); 
    
    }
    void OnDisable() {StopCoroutine(_saveCoroutine);}
    public void Save(GameData data)
    {


        data.posX = transform.position.x; 
        data.posY = transform.position.y; 
        data.posZ = transform.position.z;


        //BU FONKSIYONDAN SONRA DATA OBJESINI DEGISTIRMEYINIZ.
        _savePointLogic(ref data);
    }

    public void Load(GameData data)
    {
        this.gameObject.SetActive(data.IsSavePointActive.GetValue(ID));
    }




    IEnumerator SaveCheckCoroutine() {

        while (true) {

            if (IsTriggerReady && Physics.CheckBox(transform.position, Size * 0.5f, Quaternion.identity, PlayerLayer)) { 
                
                SaveSystem.Instance.Save();
                break;
            
            }
            

            yield return new WaitForSeconds(1f);

        
        }


        



    }

    private void OnDrawGizmos()
    {
        Gizmos.color = GizmosColor;
        Gizmos.DrawWireCube(transform.position, Size);

    }



    void _savePointLogic(ref GameData data) { 
    
        // surekli data.IsSavePointActive'e erismek yerine lokal degiskene aldim 
        SerializableDictionary<string, bool> saveData = data.IsSavePointActive; 


        //  Aktif olan save pointin ID'si kullanilarak array indexi aldim.
        int idx = saveData.TryGetKeyIdx(ID);
        // -1 donmesi bu idnin olmadigini gosterir.
        if (idx == -1) throw new Exception("IDNotFoundError");

        // Dictionarydeki en son save point aktifse baska aktif edilecek save point yoktur.
        // Oyundaki her son save pointe ulastigimizda oyun progresi resetlenir.
        if (idx == saveData.keyArr.Length - 1) {

            SaveSystem.Instance.OverrideNewData();
            this.gameObject.SetActive(false);
            Debug.Log("Game progress has been reset");
            return;

        } 
       

        // Load fonksiyonu baska sahnede savepoint varsa onu aktiflestiriyor.
        // Ancak sonraki savepoint ayni sahne icinde olabilir. 
        // Dolayisiyla asagidaki kod bu sahnedeki butun savePointleri alip sonraki save noktasi bulunduysa onun objesini aktiflestirir.
        // Aktiflesen obje OnEnable() cagirarak Coroutine'i baslatir.
        SaveTrigger[] triggersOnThisScene = GameObject.FindObjectsOfType<SaveTrigger>(true);
        saveData.SetKeyValuePair(ID, false);
        string nextKey = saveData.keyArr[idx + 1];
        saveData.SetKeyValuePair( nextKey, true);
        for (int i = 0; i < triggersOnThisScene.Length; i++) 
            if (triggersOnThisScene[i].ID == nextKey) triggersOnThisScene[i].gameObject.SetActive(true);


        data.IsSavePointActive = saveData;
        Debug.Log("This worked on " + this.gameObject.name);
        this.gameObject.SetActive(false);
    }



}
