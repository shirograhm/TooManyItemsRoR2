using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    public class TalismanStatistics : MonoBehaviour
    {
        private float _baseDamageBonus;
        private float _attackSpeedPercentBonus;
        private float _moveSpeedPercentBonus;
        private float _critChanceBonus;
        private float _critDamageBonus;
        private float _maxHealthBonus;
        private float _regenerationBonus;
        private float _shieldBonus;
        private float _armorBonus;
        
        public float BaseDamageBonus
        {
            get { return _baseDamageBonus; }
            set
            {
                _baseDamageBonus = value;
                if (NetworkServer.active)
                {
                    new Sync(
                        gameObject.GetComponent<NetworkIdentity>().netId, 
                        value, 
                        _attackSpeedPercentBonus, 
                        _moveSpeedPercentBonus, 
                        _critChanceBonus,
                        _critDamageBonus,
                        _maxHealthBonus,
                        _regenerationBonus,
                        _shieldBonus,
                        _armorBonus
                    ).Send(NetworkDestination.Clients);
                }
            }
        }
        public float AttackSpeedPercentBonus
        {
            get { return _attackSpeedPercentBonus; }
            set
            {
                _attackSpeedPercentBonus = value;
                if (NetworkServer.active)
                {
                    new Sync(
                        gameObject.GetComponent<NetworkIdentity>().netId,
                        _baseDamageBonus,
                        value,
                        _moveSpeedPercentBonus,
                        _critChanceBonus,
                        _critDamageBonus,
                        _maxHealthBonus,
                        _regenerationBonus,
                        _shieldBonus,
                        _armorBonus
                    ).Send(NetworkDestination.Clients);
                }
            }
        }
        public float MoveSpeedPercentBonus
        {
            get { return _moveSpeedPercentBonus; }
            set
            {
                _moveSpeedPercentBonus = value;
                if (NetworkServer.active)
                {
                    new Sync(
                        gameObject.GetComponent<NetworkIdentity>().netId,
                        _baseDamageBonus,
                        _attackSpeedPercentBonus,
                        value,
                        _critChanceBonus,
                        _critDamageBonus,
                        _maxHealthBonus,
                        _regenerationBonus,
                        _shieldBonus,
                        _armorBonus
                    ).Send(NetworkDestination.Clients);
                }
            }
        }
        public float CritChanceBonus
        {
            get { return _critChanceBonus; }
            set
            {
                _critChanceBonus = value;
                if (NetworkServer.active)
                {
                    new Sync(
                        gameObject.GetComponent<NetworkIdentity>().netId,
                        _baseDamageBonus,
                        _attackSpeedPercentBonus,
                        _moveSpeedPercentBonus,
                        value,
                        _critDamageBonus,
                        _maxHealthBonus,
                        _regenerationBonus,
                        _shieldBonus,
                        _armorBonus
                    ).Send(NetworkDestination.Clients);
                }
            }
        }
        public float CritDamageBonus
        {
            get { return _critDamageBonus; }
            set
            {
                _critDamageBonus = value;
                if (NetworkServer.active)
                {
                    new Sync(
                        gameObject.GetComponent<NetworkIdentity>().netId,
                        _baseDamageBonus,
                        _attackSpeedPercentBonus,
                        _moveSpeedPercentBonus,
                        _critChanceBonus,
                        value,
                        _maxHealthBonus,
                        _regenerationBonus,
                        _shieldBonus,
                        _armorBonus
                    ).Send(NetworkDestination.Clients);
                }
            }
        }
        public float MaxHealthBonus
        {
            get { return _maxHealthBonus; }
            set
            {
                _maxHealthBonus = value;
                if (NetworkServer.active)
                {
                    new Sync(
                        gameObject.GetComponent<NetworkIdentity>().netId,
                        _baseDamageBonus,
                        _attackSpeedPercentBonus,
                        _moveSpeedPercentBonus,
                        _critChanceBonus,
                        _critDamageBonus,
                        value,
                        _regenerationBonus,
                        _shieldBonus,
                        _armorBonus
                    ).Send(NetworkDestination.Clients);
                }
            }
        }
        public float RegenerationBonus
        {
            get { return _regenerationBonus; }
            set
            {
                _regenerationBonus = value;
                if (NetworkServer.active)
                {
                    new Sync(
                        gameObject.GetComponent<NetworkIdentity>().netId,
                        _baseDamageBonus,
                        _attackSpeedPercentBonus,
                        _moveSpeedPercentBonus,
                        _critChanceBonus,
                        _critDamageBonus,
                        _maxHealthBonus,
                        value,
                        _shieldBonus,
                        _armorBonus
                    ).Send(NetworkDestination.Clients);
                }
            }
        }
        public float ShieldBonus
        {
            get { return _shieldBonus; }
            set
            {
                _shieldBonus = value;
                if (NetworkServer.active)
                {
                    new Sync(
                        gameObject.GetComponent<NetworkIdentity>().netId,
                        _baseDamageBonus,
                        _attackSpeedPercentBonus,
                        _moveSpeedPercentBonus,
                        _critChanceBonus,
                        _critDamageBonus,
                        _maxHealthBonus,
                        _regenerationBonus,
                        value,
                        _armorBonus
                    ).Send(NetworkDestination.Clients);
                }
            }
        }
        public float ArmorBonus
        {
            get { return _armorBonus; }
            set
            {
                _armorBonus = value;
                if (NetworkServer.active)
                {
                    new Sync(
                        gameObject.GetComponent<NetworkIdentity>().netId,
                        _baseDamageBonus,
                        _attackSpeedPercentBonus,
                        _moveSpeedPercentBonus,
                        _critChanceBonus,
                        _critDamageBonus,
                        _maxHealthBonus,
                        _regenerationBonus,
                        _shieldBonus,
                        value
                    ).Send(NetworkDestination.Clients);
                }
            }
        }

        public class Sync : INetMessage
        {
            NetworkInstanceId objId;
            float baseDamageBonus;
            float attackSpeedPercentBonus;
            float moveSpeedPercentBonus;
            float critChanceBonus;
            float critDamageBonus;
            float maxHealthBonus;
            float regenerationBonus;
            float shieldBonus;
            float armorBonus;

            public Sync()
            {
            }

            public Sync(NetworkInstanceId objId, float baseDamage, float attackSpeed, float moveSpeed, float critChance, float critDamage, float maxHealth, float regen, float shield, float armor)
            {
                this.objId = objId;
                baseDamageBonus = baseDamage;
                attackSpeedPercentBonus = attackSpeed;
                moveSpeedPercentBonus = moveSpeed;
                critChanceBonus = critChance;
                critDamageBonus = critDamage;
                maxHealthBonus = maxHealth;
                regenerationBonus = regen;
                shieldBonus = shield;
                armorBonus = armor;
            }

            public void Deserialize(NetworkReader reader)
            {
                objId = reader.ReadNetworkId();
                baseDamageBonus = reader.ReadSingle();
                attackSpeedPercentBonus = reader.ReadSingle();
                moveSpeedPercentBonus = reader.ReadSingle();
                critChanceBonus = reader.ReadSingle();
                critDamageBonus = reader.ReadSingle();
                maxHealthBonus = reader.ReadSingle();
                regenerationBonus = reader.ReadSingle();
                shieldBonus = reader.ReadSingle();
                armorBonus = reader.ReadSingle();
            }

            public void OnReceived()
            {
                if (NetworkServer.active) return;

                GameObject obj = Util.FindNetworkObject(objId);
                if (obj != null)
                {
                    TalismanStatistics component = obj.GetComponent<TalismanStatistics>();
                    if (component != null)
                    {
                        component.BaseDamageBonus = baseDamageBonus;
                        component.AttackSpeedPercentBonus = attackSpeedPercentBonus;
                        component.MoveSpeedPercentBonus = moveSpeedPercentBonus;
                        component.CritChanceBonus = critChanceBonus;
                        component.CritDamageBonus = critDamageBonus;
                        component.MaxHealthBonus = maxHealthBonus;
                        component.RegenerationBonus = regenerationBonus;
                        component.ShieldBonus = shieldBonus;
                        component.ArmorBonus = armorBonus;
                    }
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(objId);
                writer.Write(baseDamageBonus);
                writer.Write(attackSpeedPercentBonus);
                writer.Write(moveSpeedPercentBonus);
                writer.Write(critChanceBonus);
                writer.Write(critDamageBonus);
                writer.Write(maxHealthBonus);
                writer.Write(regenerationBonus);
                writer.Write(shieldBonus);
                writer.Write(armorBonus);
            }
        }
    }

}
