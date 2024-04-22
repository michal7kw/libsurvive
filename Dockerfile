FROM ubuntu:focal
RUN mkdir /source

ARG DEBIAN_FRONTEND=noninteractive

RUN rm -f /etc/apt/apt.conf.d/docker-clean; echo 'Binary::apt::APT::Keep-Downloaded-Packages "true";' > /etc/apt/apt.conf.d/keep-cache
RUN --mount=type=cache,target=/var/cache/apt --mount=type=cache,target=/var/lib/apt \
 apt update && apt-get install --no-install-recommends -y \
    build-essential \
    udev usbutils	 \
    libtool \
    libeigen3-dev \
    git libusb-dev libusb-1.0-0-dev \
    cmake \
    zlib1g-dev \
    python3 \
    python3-pip

SHELL ["/bin/bash", "-c"]

RUN --mount=type=bind,target=/source,source=.,rw --mount=type=cache,target=/build/libsurvive,id=build-survive \
    mkdir -p /build/libsurvive && \
    cd /build/libsurvive && cmake -DCMAKE_INSTALL_PREFIX=/usr/local -DCMAKE_BUILD_TYPE=Release /source && make -j4 install

RUN mkdir -p /root/.config
RUN ldconfig

COPY python_scripts /app/python_scripts

RUN apt-get update && apt-get install -y \
    libgtk-3-dev \
    libglib2.0-dev \
    libcairo2-dev \
    libpango1.0-dev \
    libgdk-pixbuf2.0-dev \
    libatk1.0-dev \
    python3-dev \
    python3-distutils

# RUN pip3 install pysurvive

CMD ["./bin/bash"]