using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


// Kayit bilgilerine erismek/degistirmek isteyen butun Monobehaviour objeleri bu interface'i uygulamalidir.
public interface ISaveLoad { 
    
    void Save(GameData data);
    void Load(GameData data);

}



// Kayit edilen oyun bilgileri tek bir csharp objesinde toplanir.
// Bu csharp objesi daha sonrasinda serialize edilerek json forrmatina donusturulur. 
// Ardindan bu json dosyasi deserialize edilerek csharp objesine donusturulur. 
// data file'ina C:\Users\[YOUR USER NAME]\AppData\LocalLow\DefaultCompany\[UNITY PROJECT NAME] uzerinden ulasabilirsiniz. 
[Serializable]
public class GameData {


    public float posX;
    public float posY;
    public float posZ;

    public SerializableDictionary<string, bool> IsSavePointActive;

    

    public GameData() { 
    
        posX = 0; 
        posY = 0; 
        posZ = 0;

        IsSavePointActive = new SerializableDictionary<string, bool>(
            new string[] { "saveKey1", "saveKey2", "saveKey3" , "saveKey4" }
            );

        IsSavePointActive.valueArr[0] = true;


    }


}






// Dosya Okuma Yazdirma islerini bu sinif yapar.
public class FileSystem {


    string fileName;
    public string path;
    private bool IsEncrypted = false;

    public FileSystem(string fname , bool isEncrypted) { 
        
        fileName = fname;
        IsEncrypted = isEncrypted;

        string last4char = "";
        for (int i = fname.Length - 4; i < fname.Length; i++) last4char += fname[i];
        if (!last4char.Equals("json")) Debug.LogError("WrongDataFileFormatError: Your data file name should end with .json instead of " + last4char); 


        path = Path.Combine(Application.persistentDataPath, fname);


        if (!File.Exists(path)) {
            GameData data = new GameData();
            using (var writer = new BinaryWriter(File.Open(path, FileMode.CreateNew))) { 
                if (IsEncrypted) writer.Write(Encrypt(JsonUtility.ToJson(data), "AMONGUS"));
                else writer.Write(JsonUtility.ToJson(data));
            
            }


        }
        Debug.Log("Data file is saved at " + path.ToString());
    }

    public GameData LoadData() { 
   
        GameData data = null;
        using (var reader = new BinaryReader(File.Open(path , FileMode.Open))) {

            if(IsEncrypted) data = JsonUtility.FromJson<GameData>( Decrypt(reader.ReadString() , "AMONGUS")  );
            else data = JsonUtility.FromJson<GameData>(reader.ReadString());
        
        }
        
        return data;
        
    }

    public void SaveData(GameData data) {

        using (var writer = new BinaryWriter(File.Open(path, FileMode.Create))) {

            if (IsEncrypted) writer.Write(Encrypt(JsonUtility.ToJson(data), "AMONGUS")); 
            else writer.Write(JsonUtility.ToJson(data));

        } 
            
        
        Debug.Log("Game is saved");
        
    }


    private string Encrypt(string dataStr , string keyword) {

        string output = string.Empty;
        int i = 0; 
        int j = 0;


        while (i < dataStr.Length) {
            output += Convert.ToChar(Convert.ToInt16(dataStr[i]) + Convert.ToInt16(keyword[j]));
            i++; 
            j = (j+1) % keyword.Length;
        }

        return output;
            
    }

    private string Decrypt(string dataStr, string keyword) {

        string output = string.Empty;
        int i = 0;
        int j = 0;


        while (i < dataStr.Length)
        {
            output += Convert.ToChar(Convert.ToInt16(dataStr[i]) - Convert.ToInt16(keyword[j]));
            i++;
            j = (j + 1) % keyword.Length;
        }

        return output;



    }





}


public class SaveSystem : MonoBehaviour
{


    [SerializeField] private string DataFName = "gameData";
    [SerializeField] private bool IsEncrypted = false;

    GameData data;
    static FileSystem _fileSystem;



    // Awake and these instance variables indicate that this object is a persistent singleton
    private static SaveSystem _instance;
    public static SaveSystem Instance { get { return _instance; } }
    private void Awake()
    {
        if (_instance != null && _instance != this) Destroy(this.gameObject);
        else {
            _instance = GameObject.FindFirstObjectByType<SaveSystem>();
            _fileSystem = new FileSystem(DataFName, IsEncrypted); 
            DontDestroyOnLoad(_instance.gameObject);

        }

        Load();

    }
    // Singleton END



    // On First time the object is created , OnApplicationStartSave controls if the fileSystem and GameData objects are initialized properly
    // if it is not initialized , it creates new one of it.
    // After Every Scene Loading , the objects that implement ISaveLoad is collected then for each one of data object starts the execution of Load function 

    private void Start()
    {
        
    }

    public void OverrideNewData() { data = new GameData(); }


    public void Save() {

        // Eger data objesi nullsa bir sey yapma
        if (_fileSystem == null) Debug.LogError("SavingFSNullError");


        if (data == null) {
            Debug.LogError("SavingGameError: Data object is null.");
            return;
        }


        // Monobehaviour'dan kalitan ve ISaveLoad uygulayan butun nesneleri topla ve hepsinde Save() calistir.
        var dataList = FindObjectsOfType<MonoBehaviour>().OfType<ISaveLoad>();
        foreach (var dataObj in dataList)
            dataObj.Save(this.data);
        
       _fileSystem.SaveData(this.data);
    }


    public void Load() {

        if (_fileSystem == null) Debug.LogError("LoadingFSNullError");

        data = _fileSystem.LoadData();
        if (data == null) {
            Debug.LogError("LoadingGameError: Data object is null.");
            return;
        }


        // Monobehaviour'dan kalitan ve ISaveLoad uygulayan butun nesneleri topla ve hepsinde Load() calistir.
        var dataList = FindObjectsOfType<MonoBehaviour>().OfType<ISaveLoad>();
        foreach (var dataObj in dataList)
            dataObj.Load(data);


    }







}
