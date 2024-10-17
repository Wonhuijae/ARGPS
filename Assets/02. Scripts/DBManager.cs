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
    public int memoId;
    public string memo;
    public double latitude; // 위도
    public double longitude; // 경도
    public string adress; // 주소
    public string road; // 도로명
    public int roadNum; // 도로 번호

    public Memo()
    {

    }

    public Memo(string memo, double latitude, double longitude)
    {
        this.memo = memo;
        this.latitude = latitude;
        this.longitude = longitude;
    }

    public void SetMemoId(int idx)
    {
        memoId = idx;
    }

    public void SetAdress(string curAddress)
    {
        adress = curAddress;
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

    private int memoIdx;

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

    public async void WriteDB(Memo _memo)
    {
        _memo.SetMemoId(memoIdx++);
        string json = JsonUtility.ToJson(_memo);
        await dbReference.Child("Memos").Child(_memo.road).SetRawJsonValueAsync(json);
        ReadDB();
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
                        Dictionary<int, Memo> memos = new Dictionary<int, Memo>();

                        foreach(DataSnapshot s in snapshot.Children)
                        {
                            Memo m = JsonUtility.FromJson<Memo>(s.GetRawJsonValue());
                            memos.Add(m.memoId, m);
                            spawnMemo.LoadMemo(m);
                            Debug.Log(m.memoId);
                        }

                        memoIdx = memos.Count + 1;
                        Debug.Log(memos.Count);
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