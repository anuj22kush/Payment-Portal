import { Injectable } from '@angular/core';

/**
 * Generates unique client request IDs (GUIDs) for idempotent payment creation.
 * Extracted into its own service for testability (SRP).
 */
@Injectable({
    providedIn: 'root'
})
export class ClientRequestIdService {
    generate(): string {
        return crypto.randomUUID();
    }
}
