# 1. No bundled frontend in the template

Date: 2026-08-10

## Status

Accepted

## Context

A scaffolded React SPA (`ClientApp/`, orchestrated via the Aspire
AppHost) was previously included, but the frontend layer is a poor fit
for a universal backend template:

- The JavaScript ecosystem has many frameworks (React, Vue, Svelte,
  Angular, and others), and each moves quickly across major versions.
- Framework choice is highly subject to personal and team preference,
  and is often already dictated by an existing organization or project.
- A bundled SPA implies a recommendation the template does not intend to
  make, and forces consumers to delete or replace it before they can use
  their own stack.

## Decision

The template ships backend-only. It does not include a frontend
application or a specific frontend framework.

Consumers are free to add whichever frontend stack they choose, hosted
and built independently, and to consume the API like any other HTTP
backend.

As a consequence of shipping no frontend, the template also configures no CORS policy. Consumers configure CORS themselves when they add a frontend.

Concretely, this means:

- Removing the `ClientApp/` React project.
- Removing the SPA orchestration from the Aspire AppHost.
- Updating the README's Technologies and Features sections to drop React
  and the SPA/App Host references.

## Consequences

- The template stays focused, framework-agnostic, and smaller.
- It works equally well as a backend for any frontend, or as a
  standalone/headless API.
- Consumers who want a frontend must scaffold and wire it up themselves;
  the template offers no opinion or example on that integration.