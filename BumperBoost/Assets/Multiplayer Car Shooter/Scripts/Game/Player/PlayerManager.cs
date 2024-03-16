using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;

namespace MultiplayerShooter
{
    public class PlayerManager : MonoBehaviourPun, IPunObservable
    {

        /*********************** DEFAULT VARIABLES ***************************/
        [Header("Default variables :")]
        public string id;

        int car_index;

        public string name;

        public bool isOnline;

        public bool isLocalPlayer;

        public enum state : int { Start, Gamming, End };

        public state currentState = state.Start;

        public int kills;

        public bool isDead;

        public Transform cameraTotarget;

        public GameObject target;

        Vector3 targetPosition;

        [HideInInspector]
        public float h, v;

        [HideInInspector]
        public float acceleration = 0f;

        Quaternion originalRot;

        public float rotateSpeed = 50f;

        // Maximum turn rate in degrees per second.
        public float turningRate = 30f;

        float speed, currentSpeed;

        float rotate, currentRotate;

        [Header("Gravity")]
        public float gravity = 200f;

        public float rotationInDegrees = 45f;

        [Header("Front Wheels")]
        public GameObject[] frontWheels;

        [Header("Back Wheels")]
        public GameObject[] backWheels;

        Quaternion wheels00OriginalRot;

        Quaternion wheels01OriginalRot;

        [HideInInspector]
        public bool toTheLeft, toTheRight;

        [HideInInspector]
        public bool onMobileButton;

        [Header("Car Models")]
        public GameObject[] cars;

        [Header("Local Player Text Color")]
        public Color localPlayerTextColour = new Color(1f, 0f, 0f, 0.1f);  // set in inspector.

        [Header("Remote Player Text Color")]
        public Color remotePlayerTextColour = new Color(1f, 0f, 0f, 0.1f);  // set in inspector.

        [Header("Player")]
        public GameObject player;

        [Header("Sphere")]
        public Rigidbody sphere;

        [Header("All")]
        public Transform all;

        [Header("Model")]
        public Transform model;

        public GameObject currentCar;

        public GameObject[] truckSmokeParticles;

        [Header("arrow")]
        public GameObject arrow;

        [Header("Min Map Arrow")]
        public GameObject minMaparrow;

        [Header("MinMap Camera")]
        public Camera minMapCamera;

        [Header("minMapBackground")]
        public GameObject minMapBackground;


        /****************************************************************************/

        /****** SYNCHRONIZATION VARIABLES (only for Network Players) ***/
        [HideInInspector]
        public float lastSynchronizationTime;

        [HideInInspector]
        public float syncDelay;

        [HideInInspector]
        public float syncTime;

        private Vector3 currentVelocity;

        Vector3 latestPos = Vector3.zero;

        Quaternion latestRot;

        [HideInInspector]
        Vector3 syncStartPosition = Vector3.zero;

        [HideInInspector]
        Vector3 syncEndPosition = Vector3.zero;

        [HideInInspector]
        public bool onReceivedPos;


        Vector3 pos;

        bool move;

        public bool onRamp;

        public float rampPower = 500f;

        public float timeToStartGame = 10;


        /************************************************************************************/


        [Header("Effects variables :")]
        public GameObject explosionPref;  // set in inspector.


        public float nitro = 100;

        public bool onNitro;

        bool enableNitro;

        float effectsDisplayTime = 0.2f;

        GameManager gameManager;

        /*************************************************************/

        /*********************** AUDIO VARIABLES ***************************/
        [Header("AUDIO VARIABLES")]

        [Header("Audio Source")]
        public AudioSource audioSource;

        [Header("Voice Audio Source")]
        public AudioSource voiceAudioSource;

        [Header("Car Engine Audio Source")]
        public AudioSource carEngineAudioSource;

        /*********************** END AUDIO EFFECTS VARIABLES ***************************/

        //camera prefab
        public GameObject camRigPref;

        public GameObject camRig;

        public Transform cameraTotarget2;



        // Use this for initialization
        void Awake()
        {

            frontWheels[0] = null;
            frontWheels[1] = null;

            originalRot = model.transform.rotation;

            GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>() as GameManager;

            //on spawn, check if this object is controlled by the client
            if (GetComponent<PhotonView>().IsMine)
            {
                isLocalPlayer = true;

                arrow.GetComponent<MeshRenderer>().enabled = false;
                minMaparrow.GetComponent<MeshRenderer>().enabled = false;

            }
            else
            {

                GetComponentInChildren<AudioListener>().enabled = false;

                minMapCamera.enabled = false;
                minMapBackground.GetComponentInChildren<MeshRenderer>().enabled = false;
                SetState("gamming");



                //configure the  players who were already in the room for the new player
                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    if (view.ViewID.ToString().Equals(player.CustomProperties["Id"]))
                    {

                        string name = (string)player.NickName;

                        SetUpCar((int)player.CustomProperties["currentCar"]);

                        Set3DName(name);

                    }
                }

            }

        }

        /// <summary>
        ///Setup players 3D Text
        /// </summary>
        /// <param name="name"></param>
        public void Set3DName(string name)
        {

            if (GetComponentInChildren<TMP_Text>() != null)
            {
                if (view.IsMine)
                {
                    GetComponentInChildren<TMP_Text>().color = localPlayerTextColour;

                    GetComponentInChildren<TMP_Text>().text = string.Empty;
                }
                else
                {
                    GetComponentInChildren<TMP_Text>().color = remotePlayerTextColour;

                    GetComponentInChildren<TMP_Text>().text = name;
                }
            }


        }


        /// <summary>
        ///  set the new player for the players who are already in the room
        /// </summary>
        /// <param name="_id">player's id</param>
        /// <param name="_name"> player's name</param>
        /// <param name="_car">car index</param>
        [PunRPC]
        public void SetUpPlayer(string _id, string _name, int _car)
        {
            if (view.ViewID.ToString().Equals(_id))
            {

                GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
                SetState("gamming");
                id = _id;
                name = _name;
                car_index = _car;
                SetUpCar(_car);
                Set3DName(name);
                //puts the new player on the list
                gameManager.networkPlayers[view.ViewID.ToString()] = this;

            }

        }

        public void DisablePlayer()
        {

            Instantiate(explosionPref, model.transform.position, model.transform.rotation);

            //spawn death cam
            camRig = GameObject.Instantiate(camRigPref, new Vector3(0f, 0f, 0f), Quaternion.identity);
            CameraFollow cf = camRig.GetComponent<CameraFollow>();
            if (cf != null)
                camRig.GetComponent<CameraFollow>().SetTarget(gameObject.transform, cameraTotarget2);

            gameObject.SetActive(false);
        }

        public void EnablePlayer()
        {
            Destroy(camRig);
            GetComponent<PlayerHealth>().Restore();
            GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            gameObject.SetActive(true);
            gameManager.PlayerRespawn();

        }

        // Update is called once per frame
        void Update()
        {

            if (photonView.IsMine)
            {



                if (!GetComponent<PlayerHealth>().isDead && currentState.Equals(state.Gamming))
                {

                    float last_h = h;

                    if (!onMobileButton)
                    {
                        // Store the input axes.
                        h = Input.GetAxisRaw("Horizontal");

                        v = Input.GetAxisRaw("Vertical");
                    }

                    //checks if the player has stopped moving to the right
                    if (h == 0 && toTheRight)
                    {
                        toTheRight = false;
                        //clears wheel rotation
                        frontWheels[0].transform.localRotation = Quaternion.Lerp(frontWheels[0].transform.localRotation,
                        wheels00OriginalRot, Time.deltaTime * turningRate);
                        frontWheels[1].transform.localRotation = Quaternion.Lerp(frontWheels[1].transform.localRotation,
                         wheels01OriginalRot, Time.deltaTime * turningRate);

                    }

                    //checks if the player has stopped moving to the left
                    if (h == 0 && toTheLeft)
                    {
                        toTheLeft = false;
                        //clears wheel rotation
                        frontWheels[0].transform.localRotation = Quaternion.Lerp(frontWheels[0].transform.localRotation,
                         wheels00OriginalRot, Time.deltaTime * turningRate);
                        frontWheels[1].transform.localRotation = Quaternion.Lerp(frontWheels[1].transform.localRotation,
                         wheels01OriginalRot, Time.deltaTime * turningRate);


                    }


                    //if the player has moved to the right
                    if (h > 0)
                    {
                        //if the player is not already moving to the right
                        if (!toTheRight)
                        {
                            toTheRight = true;

                            //if the player is accelerating, move the wheels to the right
                            if (acceleration >= 0)
                            {
                                frontWheels[0].transform.Rotate(rotationInDegrees, 0, 0);
                                frontWheels[1].transform.Rotate(rotationInDegrees, 0, 0);

                            }
                            else if (acceleration < 0)
                            { //otherwise, if the player is reversing, move the wheels in the opposite direction
                                frontWheels[0].transform.Rotate(-rotationInDegrees, 0, 0);
                                frontWheels[1].transform.Rotate(-rotationInDegrees, 0, 0);

                            }

                        }

                    }

                    //if the player has moved to the left
                    if (h < 0)
                    {
                        //if the player is not already moving to the left
                        if (!toTheLeft)
                        {
                            toTheLeft = true;
                            if (acceleration >= 0)
                            {
                                frontWheels[0].transform.Rotate(-rotationInDegrees, 0, 0);
                                frontWheels[1].transform.Rotate(-rotationInDegrees, 0, 0);
                            }
                            else if (acceleration < 0)
                            {//otherwise, if the player is reversing, move the wheels in the opposite direction
                                frontWheels[0].transform.Rotate(rotationInDegrees, 0, 0);
                                frontWheels[1].transform.Rotate(rotationInDegrees, 0, 0);
                            }

                        }


                    }

                    // if the player presses the "space bar" key on the keyboard, it activates nitro if there is a load
                    if (Input.GetKeyUp(KeyCode.Space) || enableNitro)
                    {


                        view.RPC("ActvateNitro", RpcTarget.All);

                    }

                    // if the car is stopped it stops the animation of the wheels
                    if (acceleration.Equals(0))
                    {
                        spin[] spins = cars[car_index].GetComponentsInChildren<spin>();
                        foreach (spin s in spins)
                        {
                            s.offSpin = true;

                        }
                    }
                    else
                    { //otherwise reactivates the wheel animation
                        spin[] spins = cars[car_index].GetComponentsInChildren<spin>();
                        foreach (spin s in spins)
                        {
                            s.offSpin = false;

                        }

                    }
                    // if the nitro is on, it gives the player maximum acceleration
                    if (onNitro)
                    {
                        acceleration = currentCar.GetComponent<ModelManager>().maxAcceleration;
                    }
                    else
                    {//otherwise it normalizes the acceleration system
                        if (v > 0)
                        {
                            acceleration = currentCar.GetComponent<ModelManager>().regularAcceleration;
                        }
                        else if (v < 0)
                        { //reverse
                            acceleration = -1 * currentCar.GetComponent<ModelManager>().regularAcceleration;

                        }
                        //user stopped backing up but acceleration is still negative
                        else if (v == 0 && acceleration < 0)
                        { //normalizes the acceleration
                            acceleration = 0;
                        }
                        else
                        { //gradually decreases the acceleration
                            float breakForce = 5;
                            if (acceleration > 0 && acceleration - breakForce >= 0)
                            {
                                acceleration -= breakForce;

                            }

                        }

                    }



                    speed = acceleration;

                    var y = h * Time.deltaTime * rotateSpeed;

                    if (acceleration > 3 || acceleration < -3)
                    {
                        //rotates the model along the y axis according to user input
                        model.transform.Rotate(0, y, 0);
                    }


                    //interpolates the current speed with the previous one
                    currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.deltaTime * 12f);

                    speed = 0f;

                }

            }
            else
            { // if network remote player
              // if the car is stopped it stops the animation of the wheels
                if (acceleration.Equals(0))
                {
                    spin[] spins = cars[car_index].GetComponentsInChildren<spin>();
                    foreach (spin s in spins)
                    {
                        s.offSpin = true;

                    }
                }
                else
                { //otherwise reactivates the wheel animation
                    spin[] spins = cars[car_index].GetComponentsInChildren<spin>();
                    foreach (spin s in spins)
                    {
                        s.offSpin = false;

                    }

                }


            }


        }


        [PunRPC]
        public void AddKill()
        {
            kills += 1;
            Debug.Log("add kill for: " + name);
            if (view.IsMine)
            {
                Hashtable props = new Hashtable
          {
              {GameConstants.PLAYER_KILL,true}
          };
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            }
        }


        // Update is called once per frame
        void FixedUpdate()
        {


            if (photonView.IsMine && !GetComponent<PlayerHealth>().isDead && currentState.Equals(state.Gamming))
            {

                //manage player movement
                Move();

                if (onRamp)
                {
                    RampJump();
                    onRamp = false;
                }

            }

            if (!view.IsMine)
            {
                //Update remote player (smooth this, this looks good, at the cost of some accuracy)
                SyncedMovement();

            }


        }


        /// <summary>
        /// method for managing player movement
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        public void Move()
        {
            //Follow Collider
            player.transform.position = Vector3.Lerp(player.transform.position, sphere.transform.position, Time.deltaTime * 4);


            //Forward Acceleration
            sphere.AddForce(model.transform.forward * currentSpeed, ForceMode.Acceleration);


            //Gravity
            sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

            //Steering
            player.transform.eulerAngles = Vector3.Lerp(transform.eulerAngles,
            new Vector3(0, transform.eulerAngles.y + currentRotate, 0), Time.deltaTime * 5f);
        }

        void RampJump()
        {
            acceleration = currentCar.GetComponent<ModelManager>().maxAcceleration;
            speed = acceleration;
            //interpolates the current speed with the previous one
            var rampSpeed = Mathf.SmoothStep(currentSpeed, rampPower * speed, Time.deltaTime * 12f);
            //Forward Acceleration
            sphere.AddForce(model.transform.forward * rampSpeed, ForceMode.Acceleration);

            //anti Gravity
            sphere.AddForce(Vector3.down * -100 * gravity, ForceMode.Acceleration);

        }

        /// <summary>
        ///  In the OnPhotonSerializeView method, variables are sent or received and synchronized.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                //We own this player: send the others our data
                stream.SendNext(player.transform.position);
                stream.SendNext(model.transform.rotation);
                stream.SendNext(v);
                stream.SendNext(sphere.velocity);
                stream.SendNext(carEngineAudioSource.pitch);
                stream.SendNext(acceleration);


            }
            else
            { //Network player, receive data

                //Network player, receive player.position, rotation, v and velocity
                latestPos = (Vector3)stream.ReceiveNext();//receive position
                latestRot = (Quaternion)stream.ReceiveNext();//receive rotation
                float v = (float)stream.ReceiveNext();// receive v input
                currentVelocity = (Vector3)stream.ReceiveNext();//receive velocity
                carEngineAudioSource.pitch = (float)stream.ReceiveNext();// receive v input
                acceleration = (float)stream.ReceiveNext();// receive v input
                lastSynchronizationTime = Time.time;
                onReceivedPos = true;


            }
        }


        public void EnableKey(string _key)
        {

            onMobileButton = true;
            switch (_key)
            {

                case "UpButton":
                    v = 1;
                    break;
                case "DownButton":
                    v = -1;
                    break;
                case "RightButton":
                    h = 1;
                    break;
                case "LeftButton":
                    h = -1;
                    break;
                case "NitroButton":
                    enableNitro = true;
                    break;

            }

        }

        public void DisableKey(string _key)
        {
            onMobileButton = false;
            switch (_key)
            {
                case "UpButton":
                    v = 0;
                    break;
                case "DownButton":
                    v = 0;
                    break;
                case "RightButton":
                    h = 0;
                    break;
                case "LeftButton":
                    h = 0;
                    break;
                case "NitroButton":
                    enableNitro = false;
                    break;
            }
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////REMOTE PLAYERS METHODS ///////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// method to synchronize the position received from the server
        /// only for Network Players
        /// </summary>
        void SyncedMovement()
        {

            /*syncTime +=Time.deltaTime;

            if(onReceivedPos)
            {
              transform.position = new Vector3(Vector3.Lerp(syncStartPosition,syncEndPosition,syncTime/syncDelay).x,transform.position.y,
              Vector3.Lerp(syncStartPosition,syncEndPosition,syncTime/syncDelay).z);

            }
            */
            Vector3 projectedPosition = latestPos + currentVelocity * (Time.time - lastSynchronizationTime);
            player.transform.position = Vector3.Lerp(player.transform.position, projectedPosition, Time.deltaTime * 4);
            model.transform.rotation = Quaternion.Lerp(model.transform.rotation, latestRot, Time.deltaTime * 4);
        }

        /// <summary>
        /// method called by SpawnBullet method on gun class
        /// </summary>
        [PunRPC]
        public void DisplayEffects()
        {

            StartCoroutine("ShowEffects");

        }

        IEnumerator ShowEffects()
        {

            GetComponentInChildren<WeaponEffects>().gunLight1.enabled = true;

            GetComponentInChildren<WeaponEffects>().gunLight2.enabled = true;

            GetComponentInChildren<WeaponEffects>().gunParticles1.Stop();

            GetComponentInChildren<WeaponEffects>().gunParticles1.Play();

            GetComponentInChildren<WeaponEffects>().gunParticles2.Stop();

            GetComponentInChildren<WeaponEffects>().gunParticles2.Play();

            yield return new WaitForSeconds(effectsDisplayTime); // wait for set reload time

            DisableEffects();

        }

        public void DisableEffects()
        {

            GetComponentInChildren<WeaponEffects>().gunLight1.enabled = false;
            GetComponentInChildren<WeaponEffects>().gunLight2.enabled = false;
            GetComponentInChildren<WeaponEffects>().gunParticles1.Stop();
            GetComponentInChildren<WeaponEffects>().gunParticles2.Stop();
        }





        /// <summary>
        /// method for activate player nitro
        /// </summary>
        [PunRPC]
        public void ActvateNitro()
        {
            SpawnNitro();
        }

        /// <summary>
        /// Spawn NITRO Particles
        /// </summary>
        public void SpawnNitro()
        {

            if (!onNitro && nitro.Equals(100))
            {
                onNitro = true;
                currentCar.GetComponent<ModelManager>().nitroManager.onNitro = true;

                if (view.IsMine)
                {
                    GameCanvas.instance.onUseNitro = true;

                }

            }
            else
            {
                Debug.Log("no Nitro brow");

            }
        }

        public void StopNitro()
        {
            nitro = 0;
            onNitro = false;
            currentCar.GetComponent<ModelManager>().nitroManager.onNitro = false;
        }


        /// <summary>
        /// load the nitro
        /// </summary>
        public void AddNitro()
        {
            nitro = 100;
        }


        /// <summary>
        ///  method used to define the skin of the car chosen by the localPlayer
        /// </summary>
        /// <param name="_index"></param>
        public void SetUpCar(int _index)
        {

            currentCar = cars[_index];
            GetComponentInChildren<CarSoundController>().SetCarSoundEffects();

            // iteration used to activate and select only the wheels of the car selected by the player on the lobby screen
            for (int i = 0; i < cars.Length; i++)
            {

                MeshRenderer[] rends = cars[i].GetComponentsInChildren<MeshRenderer>();
                SkinnedMeshRenderer[] skinRends = cars[i].GetComponentsInChildren<SkinnedMeshRenderer>();
                Light[] lights = cars[i].GetComponentsInChildren<Light>();

                if (i.Equals(_index))
                {

                    foreach (MeshRenderer rend in rends)
                    {
                        rend.enabled = true;
                    }
                    foreach (SkinnedMeshRenderer skinRend in skinRends)
                    {
                        skinRend.enabled = true;
                    }
                    foreach (Light light in lights)
                    {
                        light.enabled = true;
                    }

                    spin[] wheels = cars[i].GetComponentsInChildren<spin>();

                    foreach (spin wheel in wheels)
                    {
                        if (wheel.isFront)
                        {
                            if (frontWheels[0] == null)
                            {
                                frontWheels[0] = wheel.transform.parent.gameObject;
                                wheels00OriginalRot = wheel.transform.parent.rotation;
                            }
                            else
                            {
                                frontWheels[1] = wheel.transform.parent.gameObject;
                                wheels01OriginalRot = wheel.transform.parent.rotation;
                            }


                        }
                    }

                }
                else
                {

                    foreach (MeshRenderer rend in rends)
                    {
                        rend.enabled = false;
                    }
                    foreach (SkinnedMeshRenderer skinRend in skinRends)
                    {
                        skinRend.enabled = false;
                    }
                    foreach (Light light in lights)
                    {
                        light.enabled = false;
                    }

                    spin[] spins = cars[i].GetComponentsInChildren<spin>();

                    foreach (spin s in spins)
                    {
                        s.offSpin = true;
                    }

                    if (i.Equals(2))
                    {
                        cars[i].GetComponent<ModelManager>().truckSmokeParticles[0].SetActive(false);
                        cars[i].GetComponent<ModelManager>().truckSmokeParticles[1].SetActive(false);
                    }
                }

            }


            if (!_index.Equals(2))// if is truck
            {
                for (int j = 0; j < currentCar.GetComponent<ModelManager>().truckSmokeParticles.Length; j++)
                {
                    currentCar.GetComponent<ModelManager>().truckSmokeParticles[j].SetActive(false);
                }
            }


        }




        public void SetState(string _state)
        {

            switch (_state)
            {

                case "start":
                    currentState = state.Start;
                    break;
                case "gamming":
                    currentState = state.Gamming;
                    break;
                case "end":
                    currentState = state.End;
                    break;
            }

        }

        public string GetState()
        {
            switch (currentState)
            {

                case state.Start:
                    return "start";
                    break;
                case state.Gamming:
                    return "gamming";
                    break;
                case state.End:
                    return "end";
                    break;
            }
            return string.Empty;

        }





        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////// AUDIO METHODS ///////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void PlaySound(AudioClip audioClip)
        {

            if (!voiceAudioSource.isPlaying)
            {

                voiceAudioSource.PlayOneShot(audioClip);
            }

        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////// END AUDIO METHODS  ///////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public PhotonView view { get { return GetComponent<PhotonView>(); } }

    }//END_CLASS
}//END_NAMESPACE