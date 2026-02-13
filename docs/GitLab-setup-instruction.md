# How to set up and use GitLab free Container Registry for build pipelines

1. Register a user in GitLab (using project email)

   - even if you plan to use Groups you have to create a dedicated account for this project for creating AccessTokens. Gitlab doesn't allow to create tokens scoped to a single project (it creates tokens per user). So, you have to create dedicated user for a project.

1. Create a personal access token with read/write access to container registry https://gitlab.com/-/user_settings/personal_access_tokens

![](images/gitlab-access-token.png)

1. Add secret variable `DOCKER_TOKEN` to a pipeline containing created token
1. Adjust `DOCKER_REGISTRY` and `DOCKER_USER` variables in pipeline.
