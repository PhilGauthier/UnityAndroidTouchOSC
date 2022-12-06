using OscJack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchToPosition : MonoBehaviour
{
    [SerializeField] OscConnection _connection = null;
    [SerializeField] string _oscAddress = "/unity";
    private int _idx=0;
    private DeviceOrientation _orientation;
    private float width;
    private float height;
    private Vector2 _pos;

    OscClient _client;
    // Start is called before the first frame update
    void Awake()
    {
        if (_connection != null)
            _client = OscMaster.GetSharedClient(_connection.host, _connection.port);
        else
            _client = null;       
        _idx = transform.GetSiblingIndex();
        
        InitSize();
    }

    void InitSize()
    {       
        width = (float)Screen.width / 2.0f;
        height = (float)Screen.height / 2.0f;
    }

    void OnGUI()
    {
        // Compute a fontSize based on the size of the screen width.
        GUI.skin.label.fontSize = (int)(Screen.width / 25.0f);

        GUI.Label(new Rect(20, 20, width, height * 0.125f),"Count = " + Input.touchCount);
    }

    // Update is called once per frame
    void Update()
    {
        if (_client == null) return;       

        _client.Send($"{_oscAddress}/count", Input.touchCount);
        for (int i=0;i<Input.touchCount;i++)
        {
            Touch touch = Input.GetTouch(i);            
            Vector2 pos = touch.position;
            _pos.x = (pos.x - width) / width;
            _pos.y = (pos.y - height) / height;
            _client.Send($"{_oscAddress}/p{i}_", _pos.x, _pos.y);
        }
    }
}
