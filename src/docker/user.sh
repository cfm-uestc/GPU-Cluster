#!/bin/bash
set -e
 
printf "\n\033[0;44m---> Creating SSH master user.\033[0m\n"
 
useradd -u 1005 -m -d /home/zhuxiaosu zhuxiaosu -s /bin/bash
echo "zhuxiaosu:ZXSzxs1232125" | chpasswd
# echo 'PATH="/usr/local/bin:/usr/bin:/bin:/usr/sbin"' >> /home/zhuxiaosu/.profile
 
echo "zhuxiaosu ALL=(ALL:ALL) ALL" >> /etc/sudoers
 
exec "$@"