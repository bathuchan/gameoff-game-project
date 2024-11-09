using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;







[Serializable]
public class SerializableDictionary<TKey , TValue>
{

    public TKey[] keyArr;
    public TValue[] valueArr;

    public SerializableDictionary(int count) { 
        keyArr = new TKey[count]; 
        valueArr = new TValue[count];
        
    }

    public SerializableDictionary(TKey[] keys ) {
        keyArr = keys; 
        valueArr = new TValue[keys.Length];
    
    }

    public int TryGetKeyIdx(TKey key) { for (int i = 0; i < keyArr.Length; i++) if (keyArr[i].Equals(key)) return i;  return -1; }


    public TValue GetValue(TKey key) {
        for (int i = 0; i < keyArr.Length; i++) if (keyArr[i].Equals(key)) return valueArr[i];
        return default(TValue);

    }

    public void SetKeyValuePair(TKey srcKey , TKey destKey ) {
        int i = 0;
        while (i < keyArr.Length) {
            if (srcKey.Equals(keyArr[i])) {
                keyArr[i] = destKey;
                break;
            }
            i++;
        }        
    }

    public void SetKeyValuePair(TKey srcKey, TValue destValue) { 
        int i = 0;
        while (i < keyArr.Length)
        {
            if (srcKey.Equals(keyArr[i]))
            {
                valueArr[i] = destValue;
                break;
            }
            i++;
        }
    }


    public void SetKeyValuePair(TKey srcKey, TKey destKey, TValue destValue) {
        int i = 0;
        while (i < keyArr.Length)
        {
            if (srcKey.Equals(keyArr[i]))
            {
                keyArr[i] = destKey;
                valueArr[i] = destValue;
                break;
            }
            i++;
        }


    }

    







}
