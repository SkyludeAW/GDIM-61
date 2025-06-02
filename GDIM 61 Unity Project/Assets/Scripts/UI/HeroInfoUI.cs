using UnityEngine;
using UnityEngine.UI;

public class HeroInfoUI : MonoBehaviour {
    [SerializeField] Unit hero;
    Color deadColor = new Color(0.25f, 0.25f, 0.25f);
    bool skilled;
    float skillCD;
    float elapsedSinceLastSkillCast;

    [SerializeField] Image heroImage;
    [SerializeField] HealthBarUI heroHealthUI;
    [SerializeField] HealthBarUI heroSkillCooldownUI;

    private void Start() {
        SetHero(hero);
    }

    private void SetHero(Unit hero) {
        if (this.hero != null) {
            this.hero.OnTakeDamage -= UpdateHealth;
            if (this.hero is Orion skilledHero) {
                skilledHero.OnCastSkill -= ResetSkillCooldown;
            }
        }

        this.hero = hero;

        if (hero != null) {
            hero.OnTakeDamage += UpdateHealth;
            if (hero is Orion newSkilledHero) {
                newSkilledHero.OnCastSkill += ResetSkillCooldown;
                skilled = true;
                skillCD = newSkilledHero.SkillCooldown;
                elapsedSinceLastSkillCast = Mathf.Max(newSkilledHero.SkillCooldown - newSkilledHero.RemainingSkillCooldown, 0f);
            } else 
                skilled = false;
        }
        
        Initialize();
    }

    private void Initialize() {
        if (heroImage != null) {
            heroImage.sprite = (hero != null) ? hero.BaseCard.Art : null;
            heroImage.preserveAspect = true;
        }
        heroHealthUI?.SetHealth((hero != null) ? hero.Hitpoint / hero.MaxHitPoint : 0f);
        heroSkillCooldownUI?.SetHealth((hero is Orion skilledHero) ? (1f - (skilledHero.RemainingSkillCooldown / skilledHero.SkillCooldown)) : 0f);
    }

    private void UpdateHealth() {
        heroHealthUI?.SetHealth((hero != null) ? hero.Hitpoint / hero.MaxHitPoint : 0f);
    }

    private void ResetSkillCooldown() {
        heroSkillCooldownUI?.SetHealth(0f);
        elapsedSinceLastSkillCast = 0f;
    }

    private void Update() {
        if (skilled) {
            heroSkillCooldownUI?.SetHealth(Mathf.Clamp((elapsedSinceLastSkillCast / skillCD), 0f, 1f));
            elapsedSinceLastSkillCast += Time.deltaTime;
        }

        if (hero != null && heroImage != null) {
            if (hero.IsDead) {
                heroImage.color = deadColor;
            } else {
                heroImage.color = Color.white;
            }
        }
    }
}
