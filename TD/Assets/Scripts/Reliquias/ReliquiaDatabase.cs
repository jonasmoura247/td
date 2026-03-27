using UnityEngine;

[CreateAssetMenu(fileName = "ReliquiaDatabase", menuName = "TD/Reliquias/Reliquia Database")]
public class ReliquiaDatabase : ScriptableObject
{
    public ReliquiaDef[] entradas;

    public ReliquiaDef ObterPorId(string idReliquia)
    {
        if (string.IsNullOrEmpty(idReliquia) || entradas == null)
            return null;

        for (int i = 0; i < entradas.Length; i++)
        {
            ReliquiaDef d = entradas[i];
            if (d != null && d.idReliquia == idReliquia)
                return d;
        }

        return null;
    }
}
