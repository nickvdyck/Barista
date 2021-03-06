# Barista
[![Build Status][azure-ci-badge]][azure-ci-url]

## Profiling

Set the following environment variable
```sh
MONO_ENV_OPTIONS=--profile=log:calls,alloc,output=output.mlpd,heapshot=1gc
```
More documentation can be found [here](https://www.mono-project.com/docs/debug+profile/profile/profiler/)

This will generate a file called `output.mpld` and can be found at the following location:
```sh
ls -al --block-size=MB Barista.app/Contents/Resources/output.mlpd
```

To examine this file you can use Xamarin profiler or the mrpof-report command
```sh
mprof-report Barista.app/Contents/Resources/output.mlpd
```

[azure-ci-badge]: https://dev.azure.com/vandycknick/Barista/_apis/build/status/nickvdyck.Barista?branchName=master
[azure-ci-url]: https://dev.azure.com/vandycknick/Barista/_build/latest?definitionId=9&branchName=master

