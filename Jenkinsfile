#!/usr/bin/env groovy

import groovy.json.JsonOutput

node ('aspdotnetcore_shoppingcart') {
	try {
		git url: 'https://github.com/stevebargelt/shoppingcart'
    env.GITURL_ATOMIST = sh(returnStdout: true, script: 'git config --get remote.origin.url').trim()
    env.GITSHA_ATOMIST = sh(returnStdout: true, script: 'git rev-parse HEAD').trim()
    env.GITBRANCH_ATOMIST = sh(returnStdout: true, script: 'git name-rev --always --name-only HEAD').trim().replace('remotes/origin/', '')
    notifyAtomist('STARTED', 'STARTED')
		stage('Build') {    
			sh 'dotnet restore test/shoppingcart.Tests/shoppingcart.Tests.csproj'
			sh 'dotnet test test/shoppingcart.Tests/shoppingcart.Tests.csproj'
		}
		stage('Publish') {
			sh 'dotnet publish src/shoppingcart/shoppingcart.csproj -c release -o $(pwd)/publish/'
			echo "Building: ${env.BUILD_TAG} || Build Number: ${env.BUILD_NUMBER}"
			sh "docker build -t absregistry.azurecr.io/shoppingcart:${env.BUILD_NUMBER} publish"
			withCredentials([usernamePassword(credentialsId: 'abs-acr-auth', passwordVariable: 'REGISTRY_PASSWORD', usernameVariable: 'REGISTRY_USER')]) {
				sh "docker login absregistry.azurecr.io -u='${REGISTRY_USER}' -p='${REGISTRY_PASSWORD}'"
			}
				sh "docker push absregistry.azurecr.io/shoppingcart:${env.BUILD_NUMBER}"
		}
		stage('ABS-Test') {
			docker.withServer('tcp://abs.harebrained-apps.com:2376', 'dockerTLSCerts') {
				sh "docker pull absregistry.azurecr.io/shoppingcart:${env.BUILD_NUMBER}"
				sh "docker stop shoppingcart || true && docker rm shoppingcart || true"
				sh "docker run -d --name shoppingcart -p 8007:80 absregistry.azurecr.io/shoppingcart:${env.BUILD_NUMBER}"
			}
		}
	} catch (e) {
		currentBuild.result = "FAILED"
    throw e	
	} finally {

	}

} //node
