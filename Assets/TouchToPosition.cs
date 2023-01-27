using OscJack;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEngine.UI;

public class TouchToPosition : MonoBehaviour
{
    [SerializeField] OscConnection _connection = null;
    [SerializeField] string _oscAddress = "/unity";
    [SerializeField] Material _colorMat;
    [SerializeField] Material _textureMat;
    [SerializeField] RectTransform _rectTransform;

    private int _touchCount=0;
    private Vector2[] _pos = new Vector2[11];
    private string _version = "0.2";
    private float _amplitude = .01f;
    private float _sat;
    private float _hue;
    Texture2D _texture;

    OscClient _client;
    // Start is called before the first frame update
    void Awake()
    {
        if (_connection != null)
            _client = OscMaster.GetSharedClient(_connection.host, _connection.port);
        else
            _client = null;
    }

    private void Start()
    {
        _texture = new Texture2D(10, 1, TextureFormat.RGBA32,false);
        _texture.wrapMode = TextureWrapMode.Clamp;
        _texture.filterMode = FilterMode.Point;
    }


    public void SetHost(string host)
    {
        _client.Dispose();
        _connection.host = host;
        _client = OscMaster.GetSharedClient(_connection.host, _connection.port);
    }
    
    public void SetPort(string portString)
    {
        int port;
        if (int.TryParse(portString, out port) == false)
            SetPort(port);
    }
    public void SetPort(int port)
    {
        _client.Dispose();
        _connection.port = port;
        _client = OscMaster.GetSharedClient(_connection.host, _connection.port);
    }

    public void SendAmplitude(float v)
    {
        _amplitude = v;
        _textureMat.SetFloat("_Amplitude", v);
        _client.Send($"{_oscAddress}/amplitude", v);
        //Debug.Log($"SendAmplitude: {v}");
    }

    public void SendSaturation(float v)
    {
        _sat = v;
        _textureMat.SetFloat("_Saturation", _sat);
        _colorMat.color = Color.HSVToRGB(_hue, _sat, 1);
        _client.Send($"{_oscAddress}/saturation", v);
        //Debug.Log($"SendSaturation: {v}");
    }

    public void SendHue(float v)
    {
        _hue = v;
        _textureMat.SetFloat("_Hue", _hue);
        _colorMat.color = Color.HSVToRGB(_hue, _sat,1);
        _client.Send($"{_oscAddress}/hue", v);
        //Debug.Log($"SendHue: {v}");
    }

    void OnGUI()
    {
        // Compute a fontSize based on the size of the screen width.
        GUI.skin.label.fontSize = (int)(Screen.width / 50.0f);
        GUI.Label(new Rect(70, 10, 1200, 70), $"V: {_version}"); 
        //GUI.Label(new Rect(60, 20, 1200, 60 ), _rectPixels.ToString());
        for (int i=0; i < _touchCount; i++)
        {
            GUI.Label(new Rect(70, 50 + 50 * i, 1200, 50), $"p{i}: " + _pos[i].ToString());
        }
    }

    protected bool SendPos(Vector2 p, int i)
    {
        Rect rect = _rectTransform.rect;
        rect.position = _rectTransform.anchoredPosition+ new Vector2(99, 120);
        //p -= _rectTransform.anchorMin;
        if (rect.Contains(p))
        {
            Vector2 pCentered = (p - rect.center) / rect.size;
            _client.Send($"{_oscAddress}/p{i}_", pCentered.x, pCentered.y);
            _pos[i] = pCentered;
            
            return true;
        }
        else
        {
            //Debug.Log($"{p} not in {rect}");
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_client==null) return;
        _touchCount = 0;
       

#if UNITY_EDITOR
        if (EditorApplication.isPlaying && Input.GetMouseButton(0))
        {            
            Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            SendPos(mousePos, 0);
            _touchCount++;            
        }
#endif
        for (int i=0;i<Input.touchCount;i++)
        {
            Touch touch = Input.GetTouch(i);            
            Vector2 pos = touch.position;
            if(SendPos(pos,i))
                _touchCount++; ;            
        }
        for(int i = 0; i < _touchCount; i++)
        {
            _texture.SetPixel(i, 0, new Color(_pos[i].x+.5f, _pos[i].y+.5f, 0));
        }
        _texture.Apply();
        _textureMat.SetInteger("_TouchCount", _touchCount);
        _textureMat.mainTexture = _texture;
       
        _client.Send($"{_oscAddress}/count", _touchCount);
    }
}
