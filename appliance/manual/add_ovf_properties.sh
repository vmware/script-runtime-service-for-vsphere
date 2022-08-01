#!/bin/bash

OUTPUT_PATH="../${OUTPUT_DIRECTORY}"

rm -f ${OUTPUT_PATH}/${PHOTON_APPLIANCE_NAME}.mf

sed "s/{{VERSION}}/${PHOTON_VERSION}/g" ${PHOTON_OVF_TEMPLATE} > photon.xml

if [ "$(uname)" == "Darwin" ]; then
    sed -i .bak1 's/<VirtualHardwareSection>/<VirtualHardwareSection ovf:transport="com.vmware.guestInfo">/g' ${OUTPUT_PATH}/${PHOTON_APPLIANCE_NAME}.ovf
    sed -i .bak2 "/    <\/vmw:BootOrderSection>/ r photon.xml" ${OUTPUT_PATH}/${PHOTON_APPLIANCE_NAME}.ovf
    sed -i .bak3 '/^      <vmw:ExtraConfig ovf:required="false" vmw:key="nvram".*$/d' ${OUTPUT_PATH}/${PHOTON_APPLIANCE_NAME}.ovf
    sed -i .bak4 "/^    <File ovf:href=\"${PHOTON_APPLIANCE_NAME}-file1.nvram\".*$/d" ${OUTPUT_PATH}/${PHOTON_APPLIANCE_NAME}.ovf
else
    sed -i 's/<VirtualHardwareSection>/<VirtualHardwareSection ovf:transport="com.vmware.guestInfo">/g' ${OUTPUT_PATH}/${PHOTON_APPLIANCE_NAME}.ovf
    sed -i "/    <\/vmw:BootOrderSection>/ r photon.xml" ${OUTPUT_PATH}/${PHOTON_APPLIANCE_NAME}.ovf
    sed -i '/^      <vmw:ExtraConfig ovf:required="false" vmw:key="nvram".*$/d' ${OUTPUT_PATH}/${PHOTON_APPLIANCE_NAME}.ovf
    sed -i "/^    <File ovf:href=\"${PHOTON_APPLIANCE_NAME}-file1.nvram\".*$/d" ${OUTPUT_PATH}/${PHOTON_APPLIANCE_NAME}.ovf
fi

ovftool ${OUTPUT_PATH}/${PHOTON_APPLIANCE_NAME}.ovf ${OUTPUT_PATH}/${FINAL_PHOTON_APPLIANCE_NAME}.ova
rm -rf ${OUTPUT_PATH}/${PHOTON_APPLIANCE_NAME}
rm -f photon.xml
