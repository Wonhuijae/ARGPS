using Firebase;
using Firebase.Database;
using Firebase.Extensions;
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
    public float latitude; // 위도
    public float longitude; // 경도
    public string adress; // 주소
    public string road;

    public Memo()
    {

    }

    public Memo(string memo, float latitude, float longitude)
    {
        this.memo = memo;
        this.latitude = latitude;
        this.longitude = longitude;
    }

    public void SetAdress()
    {

    }
}

public class DBManager : MonoBehaviour
{
    DatabaseReference dbReference;

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

        ReadDB();
    }

    private void WriteDB(Memo _memo)
    {
        string json = JsonUtility.ToJson(_memo);

        dbReference.Child("Memos").Child(_memo.road).SetRawJsonValueAsync(json);
    }

    public async void WriteDB(string key, string value)
    {
        await dbReference.Child("Test").Child(key).SetValueAsync(value);
    }

    private void ReadDB()
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