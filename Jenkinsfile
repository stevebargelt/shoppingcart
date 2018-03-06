#!/usr/bin/env groovy

import groovy.json.JsonOutput
import java.util.Optional

slackNotificationChannel = "development"
jobName = ""
commit = ""
author = ""
message = ""


// def isPublishingBranch = { ->
//     return env.GIT_BRANCH == 'origin/master' || env.GIT_BRANCH =~ /release.+/
// }

def isResultGoodForPublishing = { ->
    return currentBuild.result == null
}

def populateGlobalVariables = {

    jobName = "${env.JOB_NAME}"
    // Strip the branch name out of the job name (ex: "Job Name/branch1" -> "Job Name")
    jobName = jobName.getAt(0..(jobName.indexOf('/') - 1))

    commit = sh(returnStdout: true, script: 'git rev-parse HEAD')
    author = sh(returnStdout: true, script: "git --no-pager show -s --format='%an' ${commit}").trim()
    message = sh(returnStdout: true, script: 'git log -1 --pretty=%B').trim()
		azureServicebusKey = env.AZURE_SERVICEBUS_KEY
}

def notifyAzureFunction(buildColor, buildStatus) {
    
    def azFuncURL = 'https://buildwatcher.azurewebsites.net/api/TestServiceBus?code='

    def payload = JsonOutput.toJson([   
                        job: "${jobName}", 
                        build: "${env.BUILD_NUMBER}",
                        title_link: "${env.BUILD_URL}",
                        color: "${buildColor}",
                        text: "${buildStatus}\n${author}",
                        branch: "${env.GIT_BRANCH}",
                        last_commit: "${message}"
                    ])

		withCredentials([string(credentialsId: 'azServiceBusKey', variable: 'AZURE_SERVICEBUS_KEY')]) {
				sh '''
					set +x
					curl -X POST -H "Content-Type: application/json"  -d "${payload}" ${azFuncURL}$AZURE_SERVICEBUS_KEY
				'''
			}
}


node ('aspdotnetcore_shoppingcart') {
	try {

		git url: 'https://github.com/stevebargelt/shoppingcart'
		stage('Build') {    
			sh 'dotnet restore test/shoppingcart.Tests/shoppingcart.Tests.csproj'
			sh 'dotnet test test/shoppingcart.Tests/shoppingcart.Tests.csproj'
      
			populateGlobalVariables()
      def buildColor = currentBuild.result == null ? "good" : "warning"
      def buildStatus = currentBuild.result == null ? "Success" : currentBuild.result

      notifyAzureFunction(buildColor, buildStatus)		
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
		// stage('Deploy Approval') {
		// 	input "Deploy to prod?"
		// }
		// stage('Production'){
		// 	echo "TODO: Deplpoy to PROD"
		// }
	} catch (e) {
		currentBuild.result = "FAILED"
    notifyAzureFunction("danger", "FAILED")

    throw e	
	} finally {

	}

} //node
