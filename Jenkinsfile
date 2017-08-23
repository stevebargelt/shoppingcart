#!/usr/bin/env groovy

node ('aspdotnetcore_shoppingcart') {
	stage('Build') {    
		git url: 'https://github.com/stevebargelt/shoppingcart'
		sh 'dotnet restore test/shoppingcart.Tests/shoppingcart.Tests.csproj'
		sh 'dotnet test test/shoppingcart.Tests/shoppingcart.Tests.csproj'
	}
	stage('Publish') {
		sh 'dotnet publish src/shoppingcart/shoppingcart.csproj -c release -o $(pwd)/publish/'
		echo "Building: ${env.BUILD_TAG} || Build Number: ${env.BUILD_NUMBER}"
		sh "docker build -t abs-registry.harebrained-apps.com/shoppingcart:${env.BUILD_NUMBER} publish"
		withCredentials([usernamePassword(credentialsId: 'absadmin', passwordVariable: 'REGISTRY_PASSWORD', usernameVariable: 'REGISTRY_USER')]) {
			sh "docker login abs-registry.harebrained-apps.com -u='${REGISTRY_USER}' -p='${REGISTRY_PASSWORD}'"
		}
    	sh "docker push abs-registry.harebrained-apps.com/shoppingcart:${env.BUILD_NUMBER}"
	}
	stage('ABS-Test') {
		docker.withServer('tcp://abs.harebrained-apps.com:2376', 'dockerTLS') {
			sh "docker pull abs-registry.harebrained-apps.com/shoppingcart:${env.BUILD_NUMBER}"
			sh "docker stop shoppingcart || true && docker rm shoppingcart || true"
			sh "docker run -d --name shoppingcart -p 8007:80 abs-registry.harebrained-apps.com/shoppingcart:${env.BUILD_NUMBER}"
		}
	}
} //node