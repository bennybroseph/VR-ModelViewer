using UnityEngine;

public class GridLayout : MonoBehaviour
{
    [SerializeField]
    private BoxCollider m_UnitVolume;
    [SerializeField]
    [Tooltip(
        "X = Objects Per Row\n" +
        "Y = Objects Per Column\n" +
        "Z = Objects Per Layer")]
    private Vector3 m_ObjectVolume;
    [SerializeField]
    private Vector3 m_CellSize;
    [SerializeField]
    private Vector3 m_Spacing;

    private int m_PrevChildCount;

    private void OnValidate()
    {
        PositionChildren();
    }

    private void LateUpdate()
    {
        if (m_PrevChildCount == transform.childCount)
            return;

        if (m_PrevChildCount < transform.childCount)
            PositionChildren();

        m_PrevChildCount = transform.childCount;
    }

    //private void PositionChildren()
    //{
    //    var gridSize = 
    //        new Vector3(
    //            m_CellSize.x * m_Spacing.x,
    //            m_CellSize.y * m_Spacing.y,
    //            m_CellSize.z * m_Spacing.z);

    //    var currentIndex = Vector3.zero;
    //    for (var i = 0; i < transform.childCount; i++)
    //    {
    //        var currentChild = transform.GetChild(i);

    //        currentChild.transform.localPosition =
    //            new Vector3(
    //                m_CellSize.x * currentIndex.x + m_Spacing.x - gridSize.x / 2f,
    //                m_CellSize.y * currentIndex.y + m_Spacing.y - gridSize.y / 2f,
    //                m_CellSize.z * currentIndex.z + m_Spacing.z - gridSize.z / 2f);

    //        currentIndex.x++;

    //        if (currentIndex.x != 0 && currentIndex.x % m_ObjectVolume.x == 0)
    //        {
    //            currentIndex.x = 0;
    //            currentIndex.y++;
    //        }
    //        if (currentIndex.y != 0 && currentIndex.y % m_ObjectVolume.y == 0)
    //        {
    //            currentIndex.y = 0;
    //            currentIndex.z++;
    //        }
    //    }
    //}

    private void PositionChildren()
    {
        var singleObjectVolume = m_CellSize + m_Spacing;

        var gridSize = m_UnitVolume.size;

        var currentIndex = Vector3.zero;
        for (var i = 0; i < transform.childCount; i++)
        {
            var currentChild = transform.GetChild(i);

            currentChild.transform.localPosition =
                new Vector3(
                    m_CellSize.x * currentIndex.x + m_Spacing.x - gridSize.x / 2f,
                    m_CellSize.y * currentIndex.y + m_Spacing.y - gridSize.y / 2f,
                    m_CellSize.z * currentIndex.z + m_Spacing.z - gridSize.z / 2f);

            currentIndex.x++;

            if (currentIndex.x != 0 && currentIndex.x * singleObjectVolume.x % m_UnitVolume.size.x == 0)
            {
                currentIndex.x = 0;
                currentIndex.y++;
            }
            if (currentIndex.y != 0 && currentIndex.y * singleObjectVolume.y % m_UnitVolume.size.y == 0)
            {
                currentIndex.y = 0;
                currentIndex.z++;
            }
        }
    }
}
