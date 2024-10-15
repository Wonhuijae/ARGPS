using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Mapbox.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 저장될 데이터 클래스
public class Memo
{
    public string memo;
    public double latitude; // 위도
    public double longitude; // 경도
    public string adress; // 주소
    public string road;

    public Memo()
    {

    }

    public Memo(string memo, double latitude, double longitude)
    {
        this.memo = memo;
        this.latitude = latitude;
        this.longitude = longitude;
    }

    public void SetAdress()
    {

    }

    public Vector2d ConverToVector2d()
    {
        return new Vector2d(latitude, longitude);
    }
}

public class DBManager : MonoBehaviour
{
    DatabaseReference dbReference;
    public SpawnMemo spawnMemo;

    public static DBManager Instance
    {
        get
        {
            if(m_instance == null)
            {
                m_instance = FindObjectOfType<DBManager>();
            }

            return m_instance;
        }
    }
    private static DBManager m_instance;

    private void Awake()
    {
        if(Instance != this)
        {
            Destroy(gameObject);
        }

        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        

        Memo t = new Memo("평생학습원", 37.46668439003358, 126.86975035983849);
        t.adress = "경기도 광명시 철망산로 2";
        t.road = "철망산로";
        WriteDB(t);
        
        ReadDB();
    }

    public async void WriteDB(Memo _memo)
    {
        string json = JsonUtility.ToJson(_memo);
        await dbReference.Child("Memos").Child(_memo.road).SetRawJsonValueAsync(json);
    }

    private void ReadDB()
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("Memos")
            .GetValueAsync().ContinueWithOnMainThread(
                task =>
                {
                    if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        Dictionary<string, Memo> memos = new Dictionary<string, Memo>();

                        foreach(DataSnapshot s in snapshot.Children)
                        {
                            Memo m = JsonUtility.FromJson<Memo>(s.GetRawJsonValue());
                            memos.Add(m.road, m);
                            spawnMemo.LoadMemo(m);
                            Debug.Log(m.road);
                        }
                    }
                    else
                    {
                        Debug.Log("Load Fail");
                    }
                }
            );
    }

    public async void TestWriteDB(string key, string value)
    {
        await dbReference.Child("Test").Child(key).SetValueAsync(value);
    }
    private void TestReadDB()
    {
        FirebaseDatabase.DefaultInstance
           .GetReference("Test").Child("responseBody")
           .GetValueAsync().ContinueWithOnMainThread(
               task =>
               {
                   if (task.IsCompleted)
                   {
                       DataSnapshot snapshot = task.Result;
                       Debug.Log(snapshot.GetRawJsonValue());

                       string value = snapshot.GetRawJsonValue();
                       Debug.Log(value);
                   }
               }
           );
    }
}