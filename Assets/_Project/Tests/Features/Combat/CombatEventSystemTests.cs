using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Combat.Services;
using asterivo.Unity60.Features.Combat.Interfaces;
using asterivo.Unity60.Features.Combat.Components;
using asterivo.Unity60.Features.Combat.Events;
using asterivo.Unity60.Features.Combat;

namespace asterivo.Unity60.Tests.Features.Combat
{
    /// <summary>
    /// 戦闘イベントシステムの動作テスト
    /// EventManagerとCombatServiceの連携を検証
    /// </summary>
    [TestFixture]
    public class CombatEventSystemTests
    {
        private IEventManager eventManager;
        private ICombatService combatService;
        private List<object> receivedEvents;
        private int eventCount;

        [SetUp]
        public void Setup()
        {
            // ServiceLocatorをクリア
            ServiceLocator.Clear();

            // EventManagerを登録
            eventManager = new EventManager();
            ServiceLocator.Register<IEventManager>(eventManager);

            // CombatServiceを登録
            combatService = new CombatService();
            ServiceLocator.Register<ICombatService>(combatService);

            // イベントレシーバー初期化
            receivedEvents = new List<object>();
            eventCount = 0;
        }

        [TearDown]
        public void TearDown()
        {
            // イベントの購読解除
            eventManager.Clear();
            ServiceLocator.Clear();
        }

        [Test]
        public void EventManager_RaisesAndReceivesEvents()
        {
            // Arrange
            bool eventReceived = false;
            string testEventName = "TestEvent";
            object testData = new { Message = "Test" };

            eventManager.Subscribe<object>(testEventName, (data) =>
            {
                eventReceived = true;
                Assert.AreEqual(testData, data);
            });

            // Act
            eventManager.RaiseEvent(testEventName, testData);

            // Assert
            Assert.IsTrue(eventReceived);
        }

        [Test]
        public void CombatService_RaisesDamageEvent()
        {
            // Arrange
            DamageEventData capturedEvent = null;
            var target = new GameObject("Target");
            var health = target.AddComponent<HealthComponent>();

            eventManager.Subscribe<DamageEventData>(
                CombatEventNames.OnDamageDealt,
                (data) => capturedEvent = data
            );

            // Act
            combatService.DealDamage(target, 25f);

            // Assert
            Assert.IsNotNull(capturedEvent);
            Assert.AreEqual(target, capturedEvent.Target);
            Assert.AreEqual(25f, capturedEvent.ActualDamage);

            // Cleanup
            Object.DestroyImmediate(target);
        }

        [Test]
        public void CombatService_RaisesHealEvent()
        {
            // Arrange
            HealEventData capturedEvent = null;
            var target = new GameObject("Target");
            var health = target.AddComponent<HealthComponent>();
            health.TakeDamage(50f); // Damage first so we can heal

            eventManager.Subscribe<HealEventData>(
                CombatEventNames.OnHeal,
                (data) => capturedEvent = data
            );

            // Act
            combatService.HealTarget(target, 30f);

            // Assert
            Assert.IsNotNull(capturedEvent);
            Assert.AreEqual(target, capturedEvent.Target);
            Assert.AreEqual(30f, capturedEvent.HealAmount);
            Assert.AreEqual(HealType.Instant, capturedEvent.HealType);

            // Cleanup
            Object.DestroyImmediate(target);
        }

        [Test]
        public void CombatService_RaisesDeathEvent()
        {
            // Arrange
            DeathEventData capturedEvent = null;
            var target = new GameObject("Target");
            var attacker = new GameObject("Attacker");
            var health = target.AddComponent<HealthComponent>();

            eventManager.Subscribe<DeathEventData>(
                CombatEventNames.OnDeath,
                (data) => capturedEvent = data
            );

            var damageInfo = new DamageInfo(200f, attacker); // Lethal damage

            // Act
            combatService.DealDamage(target, 200f, damageInfo);

            // Assert
            Assert.IsNotNull(capturedEvent);
            Assert.AreEqual(target, capturedEvent.Source);
            Assert.AreEqual(attacker, capturedEvent.Killer);

            // Cleanup
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(attacker);
        }

        [Test]
        public void CombatService_RaisesCombatStartedEvent()
        {
            // Arrange
            bool eventReceived = false;
            var attacker = new GameObject("Attacker");
            var target = new GameObject("Target");

            eventManager.Subscribe<object>(
                CombatEventNames.OnCombatStarted,
                (data) => eventReceived = true
            );

            // Act
            combatService.StartCombat(attacker, target);

            // Assert
            Assert.IsTrue(eventReceived);

            // Cleanup
            Object.DestroyImmediate(attacker);
            Object.DestroyImmediate(target);
        }

        [Test]
        public void CombatService_RaisesCombatEndedEvent()
        {
            // Arrange
            bool eventReceived = false;
            var participant = new GameObject("Participant");
            combatService.StartCombat(participant, participant);

            eventManager.Subscribe<object>(
                CombatEventNames.OnCombatEnded,
                (data) => eventReceived = true
            );

            // Act
            combatService.EndCombat(participant);

            // Assert
            Assert.IsTrue(eventReceived);

            // Cleanup
            Object.DestroyImmediate(participant);
        }

        [Test]
        public void EventManager_UnsubscribeStopsReceivingEvents()
        {
            // Arrange
            int callCount = 0;
            System.Action<object> handler = (data) => callCount++;
            string eventName = "TestUnsubscribe";

            eventManager.Subscribe(eventName, handler);

            // Act
            eventManager.RaiseEvent(eventName, null);
            Assert.AreEqual(1, callCount);

            eventManager.Unsubscribe(eventName, handler);
            eventManager.RaiseEvent(eventName, null);

            // Assert
            Assert.AreEqual(1, callCount); // Should still be 1
        }

        [Test]
        public void EventManager_UnsubscribeAll_ClearsAllHandlers()
        {
            // Arrange
            int callCount = 0;
            string eventName = "TestUnsubscribeAll";

            eventManager.Subscribe<object>(eventName, (data) => callCount++);
            eventManager.Subscribe<object>(eventName, (data) => callCount++);
            eventManager.Subscribe<object>(eventName, (data) => callCount++);

            // Act
            eventManager.RaiseEvent(eventName, null);
            Assert.AreEqual(3, callCount);

            eventManager.UnsubscribeAll(eventName);
            eventManager.RaiseEvent(eventName, null);

            // Assert
            Assert.AreEqual(3, callCount); // Should still be 3
        }

        [Test]
        public void EventManager_TypedEvents_WorkCorrectly()
        {
            // Arrange
            DamageEventData capturedDamage = null;
            HealEventData capturedHeal = null;

            eventManager.Subscribe<DamageEventData>(
                "TypedDamage",
                (data) => capturedDamage = data
            );

            eventManager.Subscribe<HealEventData>(
                "TypedHeal",
                (data) => capturedHeal = data
            );

            var damageData = new DamageEventData(null, null, new DamageInfo(50f), 50f);
            var healData = new HealEventData(null, null, 30f, HealType.Instant);

            // Act
            eventManager.RaiseEvent("TypedDamage", damageData);
            eventManager.RaiseEvent("TypedHeal", healData);

            // Assert
            Assert.IsNotNull(capturedDamage);
            Assert.IsNotNull(capturedHeal);
            Assert.AreEqual(50f, capturedDamage.ActualDamage);
            Assert.AreEqual(30f, capturedHeal.HealAmount);
        }

        [Test]
        public void CombatService_RaisesCriticalHitEvent()
        {
            // Arrange
            bool criticalEventReceived = false;
            var target = new GameObject("Target");
            var health = target.AddComponent<HealthComponent>();

            var damageInfo = new DamageInfo(50f);
            damageInfo.isCritical = true;

            eventManager.Subscribe<DamageEventData>(
                CombatEventNames.OnCriticalHit,
                (data) => criticalEventReceived = true
            );

            // Act
            combatService.DealDamage(target, 50f, damageInfo);

            // Assert
            Assert.IsTrue(criticalEventReceived);

            // Cleanup
            Object.DestroyImmediate(target);
        }

        [Test]
        public void MultipleSubscribers_AllReceiveEvents()
        {
            // Arrange
            int subscriber1Count = 0;
            int subscriber2Count = 0;
            int subscriber3Count = 0;
            string eventName = "MultiSubscriberTest";

            eventManager.Subscribe<object>(eventName, (data) => subscriber1Count++);
            eventManager.Subscribe<object>(eventName, (data) => subscriber2Count++);
            eventManager.Subscribe<object>(eventName, (data) => subscriber3Count++);

            // Act
            eventManager.RaiseEvent(eventName, null);

            // Assert
            Assert.AreEqual(1, subscriber1Count);
            Assert.AreEqual(1, subscriber2Count);
            Assert.AreEqual(1, subscriber3Count);
        }
    }
}
