FROM {0}

RUN apt-get update && apt-get install -y apt-utils
RUN apt-get install -y openssh-server sudo vim nano

RUN useradd -u {3} -d /home/{1} {1} -s /bin/bash
RUN echo "{1}:{2}" | chpasswd
RUN echo "{1} ALL=(ALL:ALL) ALL" >> /etc/sudoers
CMD service ssh start && tail -f /dev/null
