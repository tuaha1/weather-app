using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using System.Net;
using System.IO;

public class getWeather : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI status;
    [SerializeField] TextMeshProUGUI weather;
    [SerializeField] TextMeshProUGUI lat;
    [SerializeField] TextMeshProUGUI lon;
    [SerializeField] TextMeshProUGUI temp;
    [SerializeField] TextMeshProUGUI feels;
    [SerializeField] TextMeshProUGUI location;  

    static string latitude;
    static string longitude;

    private void Awake()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
    }

    void Start()
    {
        Debug.Log("hello world");
        StartCoroutine(FetchWeatherData());
    }

    IEnumerator FetchWeatherData()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            status.text = "couldnt connect to the internet";
            yield break;
        }

        if (!Input.location.isEnabledByUser)
        {
            status.text = "turn on your location and restart the app";
            yield break;
        }

        Input.location.Start();


        int maxWait = 20;
        while(Input.location.status == LocationServiceStatus.Initializing && maxWait <= 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if(maxWait < 1)
        {
            yield break;
        }

        if(Input.location.status == LocationServiceStatus.Failed)
        {
            status.text = "failed to fetch your location";
            yield break;
        }
        else
        {
            status.text = "status: initializing...";
            InvokeRepeating(nameof(UpdateGpsLocation), 1f, 1f);
        }

    }
        
    private void UpdateGpsLocation()
    {
        if(Input.location.status == LocationServiceStatus.Running)
        {
            status.text = "status: " + Input.location.status.ToString();
            latitude = Input.location.lastData.latitude.ToString();
            longitude = Input.location.lastData.longitude.ToString();
            getWeatherInfo();
        }
    }

    public void getWeatherInfo()
    {
        string requestUrl = "http://api.openweathermap.org/data/2.5/weather?lat=" + latitude + "&lon=" + longitude + "&units=metric&appid=605fef9b9c1f491dfd9b6e9b2a207f5d";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
        HttpWebResponse respone = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(respone.GetResponseStream());
        string json = reader.ReadToEnd();
        lat.text = "Latitude: " + latitude;
        lon.text = "Longitude: " + longitude;
        weather.text = "Weather: " + JsonUtility.FromJson<weatherClass>(json).weather[0].main;
        temp.text = "Temperature: " + JsonUtility.FromJson<weatherClass>(json).main.temp;
        feels.text = "Feels Like: " + JsonUtility.FromJson<weatherClass>(json).weather[0].description;
        location.text = "Location: " + JsonUtility.FromJson<weatherClass>(json).name;

    }

}

[System.Serializable]
public class weatherClass
{
    public string name;
    public CoordList coord;
    public List<WeatherList> weather;
    public mainList main;
}

[System.Serializable]
public class WeatherList
{
    public int id;
    public string main;
    public string description;
    public string icon;
}

[System.Serializable]
public class CoordList
{
    public double lon;
    public double lat;
}

[System.Serializable]
public class mainList
{
    public double temp;
    public double feels_like;
}


