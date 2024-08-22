using Photon.Pun;
using System;

public interface IStatus
{
    #region 상태 및 능력치
    public Define.Role Role { get;  set; }
    public float Hp { get; set; }
    public float Sp { get; set; }
    public float MaxHp { get; set; }
    public float MaxSp { get; set; }
    public float Defence { get; set; }
    #endregion


    public void TakedDamage(int attack, PhotonMessageInfo info) { }
    public void TakedDamage(int attack) { }
    public void HitChangeMaterials() { }
}
