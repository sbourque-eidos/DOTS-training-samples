using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;

public class UIUtils : MonoBehaviour
{
    private Text uiText;
    private EntityManager manager;
    EntityQuery m_Group;

    // Start is called before the first frame update
    void Start()
    {
        uiText = GetComponent<Text>();
        manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        m_Group = manager.CreateEntityQuery(ComponentType.ReadOnly<UIData>());
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Group.IsCreated)
        {
            var uiData = m_Group.GetSingleton<UIData>();
            uiText.text = "Can count:" + uiData.canCount.ToString()
                + "\nBall count:" + uiData.ballCount.ToString()
                + "\nBall hits:" + uiData.ballHits.ToString();
        }
    }
}
