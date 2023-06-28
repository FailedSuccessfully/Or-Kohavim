using UnityEngine;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class VideoManager : MonoBehaviour
{
    [Range(0,1)]
    public float minBrightness=0, maxBrightness=1;
    [SerializeField] float attenuation = .1f;
    [SerializeField] float idleTime = 30;
    [SerializeField] RawImage output;
    [SerializeField] Image title;
    [SerializeField] Sprite[] titles;
    [SerializeField] Vector2[] timeStamps;
    int persistantIndex;

    
    float currentBrightness;
    VideoPlayer vid;
    PlayerInput input;
    SerialController serial;
    float timer;
    bool isIdle;

    string cfg = Application.streamingAssetsPath + "/cfg.ini";

    void Awake()
    {
        // cache component
        vid = GetComponent<VideoPlayer>();
        input = GetComponent<PlayerInput>();
        serial = GetComponent<SerialController>();

        if (File.Exists(cfg)){
            string text = File.ReadAllLines(cfg)[0];
            Debug.Log(text);
            if (text != "")
                serial.portName = text;
        }
        else{
            Debug.LogWarning($"config file {cfg} not found");
        }

    
        // assign controls
        InputActionAsset iaa = input.actions;
        iaa.FindAction("Brightness Control").performed += ctx => 
        {
            ControlBrightness(ctx.ReadValue<float>());
            TriggerInteract();
        };
        iaa.FindAction("Quit").performed += ctx => {
            Application.Quit(0);
        };
    }

    void Start()
    {
        title.CrossFadeAlpha(0, 0, true);
        currentBrightness = maxBrightness;
        ControlBrightness(currentBrightness);
        vid.Play();
        timer = idleTime;
        
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= idleTime)
            TriggleIdle();
            
    }

    void LateUpdate()
    {
        SetOutput();
        SetOutputFromAnalog();
        MonitorTime();
        Debug.Log(currentBrightness);
    }

    public void ControlBrightness(float value) {
        currentBrightness = Mathf.Clamp(currentBrightness + (value * attenuation), minBrightness, maxBrightness);
    }

    void SetOutput() {
        output.color = Color.white * currentBrightness;
        TriggerInteract();
    }
    void SetOutputFromAnalog(){
        string message = serial.ReadSerialMessage();

        if (float.TryParse(message, out float value)){
            float brightness = Mathf.Clamp(value, minBrightness, maxBrightness);
            if (brightness != currentBrightness){
                currentBrightness = brightness;
                TriggerInteract();
            }
        }
    }

    void TriggerInteract(){
        vid.Play();
        isIdle = false;
        timer = 0;
    }

    void TriggleIdle(){
        //vid.Pause();
        //isIdle = true;
    }

    void MonitorTime(){
        if (!isIdle){
            float vidTime = (float)(vid.time % vid.clip.length);
            if (vidTime > timeStamps[persistantIndex].y){
                persistantIndex++;
                persistantIndex %= titles.Length;

                title.CrossFadeAlpha(0, .3f, true);

            } else
            if (vidTime > timeStamps[persistantIndex].x){
                if (titles[persistantIndex] != null){
                    title.sprite = titles[persistantIndex];
                    title.CrossFadeAlpha(1, .3f, true);
                }
            }
        }
    }

    public void foo(){
        Debug.Log($"{currentBrightness}/{maxBrightness}");
    }
}
