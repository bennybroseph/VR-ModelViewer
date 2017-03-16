using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ResourceManager : MonoBehaviour
{
    [Header("Debugging"), SerializeField]
    private Text m_DebugText;

    private List<string> m_Files = new List<string>();

    private void LateUpdate()
    {
        if (Directory.Exists("Assets\\Resources"))
        {
            var files = Directory.GetFiles("Assets\\Resources");

            var prevCount = m_Files.Count;
            m_Files.AddRange(files.Where(file => !m_Files.Contains(file)));

            // A new Model was added to the directory
            if (m_Files.Count > prevCount && m_DebugText != null)
            {
                foreach (var file in m_Files)
                    m_DebugText.text += '\n' + file;
            }
        }
    }
}
