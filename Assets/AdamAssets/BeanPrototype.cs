using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeanPrototype : MonoBehaviour , ISaveLoad
{


    Camera cam;

    [Header("Cam Settings")]
    public float sensitivityX; 
    public float sensitivityY;

    [Header("Movement Settings")]
    public int Speed;


    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;


    }

    // Update is called once per frame
    void Update()
    {
        cam.transform.localRotation = Quaternion.Euler(

            cam.transform.localRotation.eulerAngles.x - Input.GetAxisRaw("Mouse Y") * sensitivityX * Time.deltaTime ,
            cam.transform.localRotation.eulerAngles.y + Input.GetAxisRaw("Mouse X") * sensitivityY * Time.deltaTime,
            cam.transform.localRotation.eulerAngles.z
            );

        transform.localPosition += Input.GetAxisRaw("Vertical") * cam.transform.forward * Speed * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.X)) {
            int buildIdx = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            if(buildIdx == 0) 
                UnityEngine.SceneManagement.SceneManager.LoadScene(1);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);

        }



    }

    public void Save(GameData data)
    {
        
    }

    public void Load(GameData data)
    {
        transform.position = new Vector3(data.posX, data.posY, data.posZ);
    }
}
