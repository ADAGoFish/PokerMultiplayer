using System;
using System.Collections;
using TMPro;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nickNameInputField;
    [SerializeField] private Image _image;
    [SerializeField] private Slider _stackSlider;

    private ISaveLoadSystem _saveLoadSystem;

    private void OnEnable()
    {
        _stackSlider.onValueChanged.AddListener(OnSliderValueChanged);
        _nickNameInputField.onEndEdit.AddListener(OnInputFiledEndEdit);
    }

    private void OnDisable()
    {
        _stackSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        _nickNameInputField.onEndEdit.RemoveListener(OnInputFiledEndEdit);
    }

    private void Start()
    {
        _saveLoadSystem = SaveLoadSystemFactory.Instance.Get();
        SetupUI();
        SetupBetSliderStep();
    }

    private void SetupUI()
    {
        PlayerData playerData = _saveLoadSystem.Load<PlayerData>();

        if (playerData.Equals(default(PlayerData)) == true)
        {
            //playerData.NickName = "Player";
            
            byte[] texture = TextureConverter.GetRawTexture(Resources.Load<Sprite>("Sprites/ava").texture);
            //playerData.AvatarBase64String = Convert.ToBase64String(texture);
                
            //playerData.Stack = (uint)_stackSlider.minValue;
            
            playerData = new PlayerData("Player", Convert.ToBase64String(texture), (uint)_stackSlider.minValue);
        }
        
        _nickNameInputField.text = playerData.NickName;

        byte[] rawTexture = Convert.FromBase64String(playerData.AvatarBase64String);
        _image.sprite = TextureConverter.GetSprite(rawTexture);

        _stackSlider.value = playerData.Stack;
        
        SavePlayerData();
    }

    private void SetupBetSliderStep()
    {
        if (_stackSlider.TryGetComponent(out ISliderSetter sliderSetter) == true)
        {
            sliderSetter.IntervalPerScroll = _stackSlider.maxValue / _stackSlider.minValue;
        }
    }
    
    private void OnInputFiledEndEdit(string value)
    {
        if (string.IsNullOrEmpty(value) == true)
        {
            _nickNameInputField.text = "Player";
        }
        
        SavePlayerData();
    }

    private void OnSliderValueChanged(float value)
    {
        SavePlayerData();
    }
   
    private void OnChangeImageButtonClick()
    {
        StartCoroutine(ChangeImage());
    }

    private IEnumerator ChangeImage()
    {
#if UNITY_EDITOR // todo
        string filePath = EditorUtility.OpenFilePanel("Select avatar", "", "jpg,png");
        yield return StartCoroutine(SetImageFromLocalFIle(filePath));

        SavePlayerData();
#endif

        yield return null;
    }

    private IEnumerator SetImageFromLocalFIle(string filePath)
    {
        using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(filePath);
        
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Log.WriteToFile(webRequest.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
            _image.sprite = TextureConverter.GetSprite(texture);
        }
    }

    private void SavePlayerData()
    {
        byte[] rawTexture = TextureConverter.GetRawTexture(_image.sprite.texture);
        PlayerData playerData = new(_nickNameInputField.text, Convert.ToBase64String(rawTexture), (uint)_stackSlider.value);
        _saveLoadSystem.Save(playerData);
    }
}