foreach(z_vcpkg_curl_component IN ITEMS SSL IPv6 unixsockets libz AsynchDNS Largefile alt-svc HSTS DICT FILE FTP FTPS GOPHER GOPHERS HTTP HTTPS IMAP IMAPS MQTT POP3 POP3S RTSP SMTP SMTPS TELNET TFTP)
  if(z_vcpkg_curl_component MATCHES "^[-_a-zA-Z0-9]*$")
    set(CURL_${z_vcpkg_curl_component}_FOUND TRUE)
  endif()
endforeach()
