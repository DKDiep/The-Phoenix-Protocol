#!/bin/bash
log=$(curl -u 72b4a34a823576b4a2977921b473401d: 'https://build-api.cloud.unity3d.com/api/v1/orgs/HEMSWORTH/projects/space-game/buildtargets/default-windows-desktop-64-bit/builds?buildStatus=success' -H 'DNT: 1' -H 'Accept-Encoding: gzip, deflate, sdch' -H 'Accept-Language: en-US,en;q=0.8' -H 'User-Agent: Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36' -H 'Content-Type: application/x-www-form-urlencoded; charset=UTF-8' -H 'Accept: */*' -H 'Referer: https://build-api.cloud.unity3d.com/docs/1.0.0/index.html' -H 'X-Requested-With: XMLHttpRequest' -H 'If-None-Match: "882003452"' -H 'Connection: keep-alive' --compressed);
id=$(echo $log | jq '.[0].build');
commit=$(echo $log | jq '.[0].lastBuiltRevision');
body=$(curl -u 72b4a34a823576b4a2977921b473401d:  "https://build-api.cloud.unity3d.com/api/v1/orgs/hemsworth/projects/space-game/buildtargets/default-windows-desktop-64-bit/builds/$id/log");
err=$(echo $body | grep 'Compilation failed' | wc -l);

commit1=${commit:1:40};

seenbefore=$(grep $commit1 commit_hashes.txt | wc -l);
if [ $seenbefore = 0 ]; then
  echo $commit1 >> commit_hashes.txt;
  echo $commit1;
  if [ $err = 0 ]; then
    echo "Success!";
    curl "https://api.bitbucket.org/2.0/repositories/pyrolite/game/commit/$commit1/statuses/build" -H 'Content-Type: application/x-www-form-urlencoded' -H 'Accept: */*' -H 'Connection: keep-alive' -H 'DNT: 1' --data $'state=SUCCESSFUL&key=frank&url=https%3A%2F%2Fbitbucket.org%2Ffrank_hemsworth%2F&name=Unity Cloud Testing&description=Built successfully' --compressed
  else
    echo "Failed!";
    curl "https://api.bitbucket.org/2.0/repositories/pyrolite/game/commit/$commit1/statuses/build" -H 'Content-Type: application/x-www-form-urlencoded' -H 'Accept: */*' -H 'Connection: keep-alive' -H 'DNT: 1' --data $'state=FAILED&key=frank&url=https%3A%2F%2Fbitbucket.org%2Ffrank_hemsworth%2F&name=Unity Cloud Testing&description=Failed test. See https://build-api.cloud.unity3d.com/api/v1/orgs/hemsworth/projects/space-game/buildtargets/default-windows-desktop-64-bit/builds/$id/log for more details.' --compressed
  fi
else
  echo "Seen commit before, not doing anything...";
fi
