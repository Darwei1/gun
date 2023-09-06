using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Animations;
public class ProjectileGun : MonoBehaviour
{
    
        private Animator anim;

        public GameObject Bullet;

        public float shootForce, upwardForce;

        public float timeBetweenShooting, Spread, reloadTime, timeBetweenShoots;
        public int magazineSize, bulletPerTap;
        public bool allowButtonHold;

        public GameObject muzzleFlash;
        public TextMeshProUGUI ammunitionDisplay;

        int bulletsLeft, bulletShot;

        bool shooting, readyToShoot, reloading;
        public Camera fpsCam;
        public Transform attackPoint;

        public bool allowInvoke = true;
 
        private void Awake(){
            bulletsLeft = magazineSize;
            readyToShoot = true;

            anim = GetComponent<Animator>();
        }

        private void Update() {
            MyInput();

            if(ammunitionDisplay != null)
                ammunitionDisplay.SetText(bulletsLeft / bulletPerTap + "  /  " + magazineSize / bulletPerTap); 
        }

        private void MyInput() {
            if(allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
            else shooting = Input.GetKeyDown(KeyCode.Mouse0);

            if(Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) {
                Reload();
                anim.SetTrigger("reload");
            }
            

            if(readyToShoot && shooting && !reloading && bulletsLeft <= 0) {
                anim.SetTrigger("reload");
                Reload();
            }


            if(readyToShoot && shooting && !reloading && bulletsLeft > 0) {
                bulletShot = 0;
                anim.SetTrigger("shoot");
                Shoot();
            }
        }

    private void Shoot() {
        readyToShoot = false;
        
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f,0.5f,0));
        RaycastHit hit;

        Vector3 targetPoint;
        if(Physics.Raycast(ray, out hit)) 
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75);

        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        float x = Random.Range(-Spread, Spread);
        float y = Random.Range(-Spread, Spread);

        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x,y,0);

        GameObject currentBullet = Instantiate(Bullet, attackPoint.position, Quaternion.identity);

        currentBullet.transform.forward = directionWithSpread.normalized;

        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(fpsCam.transform.up * upwardForce, ForceMode.Impulse);

        if  (muzzleFlash != null)
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);

        bulletsLeft--;
        bulletShot++;

        if(allowInvoke) {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }

        if(bulletShot < bulletPerTap && bulletsLeft> 0)
            Invoke("Shoot", timeBetweenShoots);
    }

    private void ResetShot() {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload() {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
        anim.SetTrigger("Reload");
    }

    private void ReloadFinished() {
        bulletsLeft = magazineSize;
        reloading = false;
        anim.SetTrigger("Idle");
    }
}
