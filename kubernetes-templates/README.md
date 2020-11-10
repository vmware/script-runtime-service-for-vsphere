# Kuberenetes templates helpers to manage "Script Runtime Service for vSphere" deployment

SRS is a kubernetes deployment that manages its registration to VC Server with kuberntes jobs. The purpose of theses kubernetes templates helpers is to manage kubernetes deployment without helm chart. A good example is if you delete the SRS kubernetes resources without using `helm delete` which runs a jop to remove the SRS VC registration. In this case SRS deployment would be removed but the VC registration will remain in the target VC server. You can use a template to create kubernetes job that performs SRS VC registration cleanup.

## srs-vc-registration-cleanup.yaml
A template with kubernetes job that performs VC registration cleanup. It runs a job from the srs-setup container which performs SRS registration cleanup actions. The job is created in the `default` runspaces. In order to run that you need to replace the template values for
1. image name and image tag
2. VC address
3. VC user
4. VC passowrd

### Example target yaml file
apiVersion: batch/v1
kind: Job
metadata:
  name: "srs-vc-registration-cleanup"
spec:
  template:
    spec:
      containers:
      - image: srs-setup:1.0
        name: srs-setup
        imagePullPolicy: IfNotPresent
        command: ["/app/service/setup"]
        env:
          - name: RUN
            value: CleanupVCRegistration
          - name: TARGET_VC_SERVER
            value: "10.23.80.118"
          - name: TARGET_VC_USER
            value: "administrator@vsphere.local"
          - name: TARGET_VC_PASSWORD
            value: "Pa$$w0rd"
          - name: TARGET_VC_INSECURE
            value: "True"
      restartPolicy: Never


### To run the job use
`kubect apply -f srs-vc-registration-cleanup.yaml`

### Once job is completed you can the delete the the kubernetes resources created by the job with
`kubect apply -f srs-vc-registration-cleanup.yaml`