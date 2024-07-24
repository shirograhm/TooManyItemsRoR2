using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TooManyItems
{
    public class HorseshoeStatistics : MonoBehaviour
    {
        private float _maxHealthBonus;
        private float _baseDamageBonus;
        private float _attackSpeedPercentBonus;
        private float _critChanceBonus;
        private float _critDamageBonus;
        private float _armorBonus;
        private float _regenerationBonus;
        private float _shieldBonus;
        private float _moveSpeedPercentBonus;

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
                        value,
                        _baseDamageBonus,
                        _attackSpeedPercentBonus,
                        _critChanceBonus,
                        _critDamageBonus,
                        _armorBonus,
                        _regenerationBonus,
                        _shieldBonus,
                        _moveSpeedPercentBonus
                    ).Send(NetworkDestination.Clients);
                }
            }
        }
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
                        _maxHealthBonus,
                        value,
                        _attackSpeedPercentBonus,
                        _critChanceBonus,
                        _critDamageBonus,
                        _armorBonus,
                        _regenerationBonus,
                        _shieldBonus,
                        _moveSpeedPercentBonus
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
                        _maxHealthBonus,
                        _baseDamageBonus,
                        value,
                        _critChanceBonus,
                        _critDamageBonus,
                        _armorBonus,
                        _regenerationBonus,
                        _shieldBonus,
                        _moveSpeedPercentBonus
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
                        _maxHealthBonus,
                        _baseDamageBonus,
                        _attackSpeedPercentBonus,
                        value,
                        _critDamageBonus,
                        _armorBonus,
                        _regenerationBonus,
                        _shieldBonus,
                        _moveSpeedPercentBonus
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
                        _maxHealthBonus,
                        _baseDamageBonus,
                        _attackSpeedPercentBonus,
                        _critChanceBonus,
                        value,
                        _armorBonus,
                        _regenerationBonus,
                        _shieldBonus,
                        _moveSpeedPercentBonus
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
                        _maxHealthBonus,
                        _baseDamageBonus,
                        _attackSpeedPercentBonus,
                        _critChanceBonus,
                        _critDamageBonus,
                        value,
                        _regenerationBonus,
                        _shieldBonus,
                        _moveSpeedPercentBonus
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
                        _maxHealthBonus,
                        _baseDamageBonus,
                        _attackSpeedPercentBonus,
                        _critChanceBonus,
                        _critDamageBonus,
                        _armorBonus,
                        value,
                        _shieldBonus,
                        _moveSpeedPercentBonus
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
                        _maxHealthBonus,
                        _baseDamageBonus,
                        _attackSpeedPercentBonus,
                        _critChanceBonus,
                        _critDamageBonus,
                        _armorBonus,
                        _regenerationBonus,
                        value,
                        _moveSpeedPercentBonus
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
                        _maxHealthBonus,
                        _baseDamageBonus,
                        _attackSpeedPercentBonus,
                        _critChanceBonus,
                        _critDamageBonus,
                        _armorBonus,
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
            float maxHealthBonus;
            float baseDamageBonus;
            float attackSpeedPercentBonus;
            float critChanceBonus;
            float critDamageBonus;
            float armorBonus;
            float regenerationBonus;
            float shieldBonus;
            float moveSpeedPercentBonus;

            public Sync()
            {
            }

            public Sync(NetworkInstanceId objId,
                float maxHealth, float baseDamage, float attackSpeed, float critChance, float critDamage,
                float armor, float regen, float shield, float moveSpeed)
            {
                this.objId = objId;
                maxHealthBonus = maxHealth;
                baseDamageBonus = baseDamage;
                attackSpeedPercentBonus = attackSpeed;
                critChanceBonus = critChance;
                critDamageBonus = critDamage;
                armorBonus = armor;
                regenerationBonus = regen;
                shieldBonus = shield;
                moveSpeedPercentBonus = moveSpeed;
            }

            public void Deserialize(NetworkReader reader)
            {
                objId = reader.ReadNetworkId();
                maxHealthBonus = reader.ReadSingle();
                baseDamageBonus = reader.ReadSingle();
                attackSpeedPercentBonus = reader.ReadSingle();
                critChanceBonus = reader.ReadSingle();
                critDamageBonus = reader.ReadSingle();
                armorBonus = reader.ReadSingle();
                regenerationBonus = reader.ReadSingle();
                shieldBonus = reader.ReadSingle();
                moveSpeedPercentBonus = reader.ReadSingle();
            }

            public void OnReceived()
            {
                if (NetworkServer.active) return;

                GameObject obj = Util.FindNetworkObject(objId);
                if (obj != null)
                {
                    HorseshoeStatistics component = obj.GetComponent<HorseshoeStatistics>();
                    if (component != null)
                    {
                        component.MaxHealthBonus = maxHealthBonus;
                        component.BaseDamageBonus = baseDamageBonus;
                        component.AttackSpeedPercentBonus = attackSpeedPercentBonus;
                        component.CritChanceBonus = critChanceBonus;
                        component.CritDamageBonus = critDamageBonus;
                        component.ArmorBonus = armorBonus;
                        component.RegenerationBonus = regenerationBonus;
                        component.ShieldBonus = shieldBonus;
                        component.MoveSpeedPercentBonus = moveSpeedPercentBonus;
                    }
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(objId);
                writer.Write(maxHealthBonus);
                writer.Write(baseDamageBonus);
                writer.Write(attackSpeedPercentBonus);
                writer.Write(critChanceBonus);
                writer.Write(critDamageBonus);
                writer.Write(armorBonus);
                writer.Write(regenerationBonus);
                writer.Write(shieldBonus);
                writer.Write(moveSpeedPercentBonus);
            }
        }
    }
}
