using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ����� ������ Ŭ����
public class Memo
{
    public string memo;
    public float latitude; // ����
    public float longitude; // �浵
    public string adress; // �ּ�
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

        
    }

    private void WriteNewStore(Memo _memo)
    {
        string json = JsonUtility.ToJson(_memo);

        dbReference.Child("Memos").Child(_memo.road).SetRawJsonValueAsync(json);
    }
}
