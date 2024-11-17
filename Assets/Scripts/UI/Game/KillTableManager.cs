using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Player;
using TMPro;

namespace ProjectUI
{
    public class KillTableManager : NetworkBehaviour
    {
        public Transform killTable; // Parent container for kill table rows
        [Networked] public int rowIndex { get; set; }
        public int rowAmount;

        public override void Spawned()
        {
            rowIndex = 0;
        }

        private void SlideRowsToTheTop()
        {
            for (int i = 0; i < rowAmount - 1; i++)
            {
                // Move the row data upwards
                string killerUsrBottom = killTable.GetChild(i + 1).GetChild(0).GetComponent<TextMeshProUGUI>().text;
                string deathUsrBottom = killTable.GetChild(i + 1).GetChild(2).GetComponent<TextMeshProUGUI>().text;

                killTable.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().text = killerUsrBottom;
                killTable.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = "Killed";
                killTable.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().text = deathUsrBottom;
            }

            // Clear the last row
            ClearRow(rowAmount - 1);
        }

        private void ClearRow(int row)
        {
            killTable.GetChild(row).GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
            killTable.GetChild(row).GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            killTable.GetChild(row).GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void AddRawToKilltableRpc(string killerUsr, string deathUsr)
        {
            if(Object.HasStateAuthority)
                rowIndex++;

            if (rowIndex > rowAmount - 1)
            {
                SlideRowsToTheTop();
                rowIndex = rowAmount - 1;
            }

            // Update the current row
            GameObject newRow = killTable.GetChild(rowIndex).gameObject;
            newRow.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = killerUsr;
            newRow.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Killed";
            newRow.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = deathUsr;

            StartCoroutine(DeleteRaw(newRow.transform));
        }

        public IEnumerator DeleteRaw(Transform deleteRaw)
        {
            yield return new WaitForSeconds(5);

            deleteRaw.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
            deleteRaw.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            deleteRaw.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
            rowIndex--;
        }
    }

}
