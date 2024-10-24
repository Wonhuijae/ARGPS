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
    public string address; // 주소
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
        address = curAddress;
    }

    public Vector2d ConverToVector2d()
    {
        return new Vector2d(latitude, longitude);
    }

    public bool Equals(Memo other)
    {
        if(other == null) return false;

        return memo == other.memo &&
            latitude == other.latitude &&
            longitude == other.longitude &&
            address == other.address &&
            road == other.road &&
            roadNum == other.roadNum;
    }
}

public class DBManager : MonoBehaviour
{
    DatabaseReference dbReference;
    public SpawnMemo spawnMemo;

    private int memoIdx;

    AppManager appInstance;

    List<Memo> memoInDB;

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
        appInstance = AppManager.Instance;
        // TestData();

        memoInDB = new();
    }

    public async void WriteDB(Memo _memo)
    {
        if (!IsCanWrite(_memo)) return;

        _memo.SetMemoId(memoIdx++);
        string json = JsonUtility.ToJson(_memo);
        await dbReference.Child("Memos").Child(_memo.road).Child(_memo.memoId.ToString()).SetRawJsonValueAsync(json);
        appInstance.ShowToastMsg("메모 저장 완료!");
        memoInDB.Add(_memo);
        spawnMemo.LoadMemo(_memo);
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

                        foreach(DataSnapshot road in snapshot.Children)
                        {
                            foreach (DataSnapshot id in road.Children)
                            {
                                Memo m = JsonUtility.FromJson<Memo>(id.GetRawJsonValue());
                                memos.Add(m.memoId, m);
                                spawnMemo.LoadMemo(m);
                            }
                        }

                        memoIdx = memos.Count + 1;
                    }
                    else
                    {
                        Debug.Log("Load Fail");
                    }
                }
            );
    }

    private bool IsCanWrite(Memo createdMemo)
    {
        foreach(var i in memoInDB)
        {
            if (i.Equals(createdMemo)) return false;
        }

        return true;
    }


    void TestData()
    {
        // 37.4773667835683, 126.862609330332
        Memo Test1 = new Memo("치과", 37.4773667835683, 126.86260933033213);
        Memo Test2 = new Memo("집", 37.465673731942026, 126.86837305178426);
        Memo Test3 = new Memo("횡단보도", 37.47689399460678, 126.86281652070888);
        Memo Test4 = new Memo("철산역", 37.47616126242499, 126.86817094843643);

        SetPropertits(Test1, 1, "경기도 광명시 오리로 902", "오리로", 902);
        SetPropertits(Test2, 2, "경기도 광명시 오리로 801", "오리로", 801);
        SetPropertits(Test3, 3, "경기도 광명시 시청로 20", "시청로", 20);
        SetPropertits(Test4, 4, "경기도 광명시 철산로 13", "철산로", 13);

        WriteDB(Test1);
        WriteDB(Test2);
        WriteDB(Test3);
        WriteDB(Test4);
    }

    void SetPropertits(Memo _memo, int _id, string _adress, string _road, int _roadNum)
    {
        _memo.memoId = _id;
        _memo.address = _adress;
        _memo.road = _road;
        _memo.roadNum = _roadNum;
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