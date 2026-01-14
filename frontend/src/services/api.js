const API_BASE_URL = 'http://localhost:5280/api';

// JWT Token'ı header'lara eklemek için yardımcı fonksiyon
const getAuthHeaders = () => {
  const token = localStorage.getItem('token');
  return {
    'Content-Type': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` })
  };
};

export const api = {
  // Users
  async getUsers() {
    const response = await fetch(`${API_BASE_URL}/users`);
    return response.json();
  },

  async getUserById(id) {
    const response = await fetch(`${API_BASE_URL}/users/${id}`);
    return response.json();
  },

  async createUser(userData) {
    const response = await fetch(`${API_BASE_URL}/users`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(userData),
    });
    return response.json();
  },

  async updateUserProfile(userId, userData) {
    const response = await fetch(`${API_BASE_URL}/users/${userId}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(userData),
    });
    if (!response.ok) throw new Error('Failed to update profile');
    return response.status === 204;
  },

  // Posts
  async getPosts(count = 20) {
    const response = await fetch(`${API_BASE_URL}/posts?count=${count}`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return response.json();
  },

  async getPostById(id) {
    const response = await fetch(`${API_BASE_URL}/posts/${id}`);
    if (!response.ok) {
      if (response.status === 404) {
        throw new Error('Post not found');
      }
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return response.json();
  },

  async getPostsByUser(userId) {
    const response = await fetch(`${API_BASE_URL}/posts/user/${userId}`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return response.json();
  },

  async getPostsByPlace(placeId) {
    const response = await fetch(`${API_BASE_URL}/posts/place/${placeId}`);
    if (!response.ok) throw new Error('Yer gönderileri alınamadı');
    return response.json();
  },

  async createPost(postData) {
    const response = await fetch(`${API_BASE_URL}/posts`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(postData),
    });
    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(errorText || 'Gönderi oluşturulamadı');
    }
    return response.json();
  },

  async updatePost(id, postData) {
    const response = await fetch(`${API_BASE_URL}/posts/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(postData),
    });
    if (!response.ok) throw new Error('Failed to update post');
    return response.status === 204;
  },

  async deletePost(id) {
    const response = await fetch(`${API_BASE_URL}/posts/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });
    if (!response.ok) throw new Error('Failed to delete post');
    return response.status === 204;
  },

  // Places
  async getPlaces() {
    const response = await fetch(`${API_BASE_URL}/places`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return response.json();
  },

  // Map / Clusters
  async getMapClusters(bounds, zoom = 10) {
    const params = new URLSearchParams();
    if (bounds?.north != null) params.append('north', bounds.north);
    if (bounds?.south != null) params.append('south', bounds.south);
    if (bounds?.east != null) params.append('east', bounds.east);
    if (bounds?.west != null) params.append('west', bounds.west);
    params.append('zoom', zoom);

    const query = params.toString();
    const url = query ? `${API_BASE_URL}/map/clusters?${query}` : `${API_BASE_URL}/map/clusters`;
    const response = await fetch(url);
    if (!response.ok) throw new Error('Harita cluster verisi alınamadı');
    return response.json();
  },

  async getPlaceById(id) {
    const response = await fetch(`${API_BASE_URL}/places/${id}`);
    return response.json();
  },

  async searchPlaces(term) {
    const response = await fetch(`${API_BASE_URL}/places/search?term=${term}`);
    return response.json();
  },

  async createPlace(placeData) {
    const response = await fetch(`${API_BASE_URL}/places`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(placeData),
    });
    return response.json();
  },

  // Comments
  async getCommentsByPost(postId) {
    const response = await fetch(`${API_BASE_URL}/comments/post/${postId}`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return response.json();
  },

  async getCommentsByUser(userId) {
    const response = await fetch(`${API_BASE_URL}/comments/user/${userId}`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return response.json();
  },

  async createComment(commentData) {
    const response = await fetch(`${API_BASE_URL}/comments`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(commentData),
    });
    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(errorText || 'Yorum oluşturulamadı');
    }
    return response.json();
  },

  // Likes
  async addLike(postId, userId) {
    const response = await fetch(`${API_BASE_URL}/likes?postId=${postId}&userId=${userId}`, {
      method: 'POST',
      headers: getAuthHeaders(),
    });
    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(errorText || 'Beğeni eklenemedi');
    }
    return response.json();
  },

  async removeLike(postId, userId) {
    const response = await fetch(`${API_BASE_URL}/likes?postId=${postId}&userId=${userId}`, {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });
    return response.status === 204;
  },

  async isLiked(postId, userId) {
    const response = await fetch(`${API_BASE_URL}/likes/post/${postId}/user/${userId}/check`);
    if (!response.ok) {
      // 404 durumunda false döndür (henüz beğenilmemiş)
      if (response.status === 404) {
        return false;
      }
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    const result = await response.json();
    return result === true;
  },

  async getLikesByUser(userId) {
    const response = await fetch(`${API_BASE_URL}/likes/user/${userId}`);
    if (!response.ok) throw new Error('Failed to fetch likes');
    return response.json();
  },

  async getLikesByPost(postId) {
    const response = await fetch(`${API_BASE_URL}/likes/post/${postId}`);
    if (!response.ok) throw new Error('Failed to fetch likes');
    return response.json();
  },

  // File Upload
  async uploadImage(file) {
    const formData = new FormData();
    formData.append('file', file);
    const token = localStorage.getItem('token');
    const response = await fetch(`${API_BASE_URL}/fileupload/image`, {
      method: 'POST',
      headers: token ? { 'Authorization': `Bearer ${token}` } : {},
      body: formData,
    });
    return response.json();
  },

  // Follow
  async followUser(followerId, followingId) {
    const response = await fetch(`${API_BASE_URL}/follows/${followerId}/follow/${followingId}`, {
      method: 'POST',
      headers: getAuthHeaders(),
    });
    return response.json();
  },

  async unfollowUser(followerId, followingId) {
    const response = await fetch(`${API_BASE_URL}/follows/${followerId}/unfollow/${followingId}`, {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });
    return response.status === 204;
  },

  async isFollowing(followerId, followingId) {
    const response = await fetch(`${API_BASE_URL}/follows/${followerId}/is-following/${followingId}`);
    return response.json();
  },

  async getFollowers(userId) {
    const response = await fetch(`${API_BASE_URL}/follows/${userId}/followers`);
    return response.json();
  },

  async getFollowing(userId) {
    const response = await fetch(`${API_BASE_URL}/follows/${userId}/following`);
    return response.json();
  },

  async getFollowStats(userId) {
    const response = await fetch(`${API_BASE_URL}/follows/${userId}/stats`);
    return response.json();
  },

  // Notifications
  async getNotifications(userId) {
    const response = await fetch(`${API_BASE_URL}/notifications/${userId}`);
    return response.json();
  },

  async getUnreadNotifications(userId) {
    const response = await fetch(`${API_BASE_URL}/notifications/${userId}/unread`);
    return response.json();
  },

  async getUnreadCount(userId) {
    const response = await fetch(`${API_BASE_URL}/notifications/${userId}/unread-count`);
    return response.json();
  },

  async markAsRead(notificationId) {
    const response = await fetch(`${API_BASE_URL}/notifications/${notificationId}/read`, {
      method: 'PUT',
    });
    return response.status === 204;
  },

  async markAllAsRead(userId) {
    const response = await fetch(`${API_BASE_URL}/notifications/${userId}/read-all`, {
      method: 'PUT',
    });
    return response.status === 204;
  },

  // Profile
  async getProfile(userId) {
    const response = await fetch(`${API_BASE_URL}/profiles/${userId}`);
    if (!response.ok) {
      if (response.status === 404) {
        throw new Error('Profile not found');
      }
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return response.json();
  },

  // Search
  async searchAll(term) {
    const response = await fetch(`${API_BASE_URL}/search/all?term=${term}`);
    return response.json();
  },

  async searchPosts(filters) {
    const params = new URLSearchParams();
    if (filters.term) params.append('term', filters.term);
    if (filters.city) params.append('city', filters.city);
    if (filters.fromDate) params.append('fromDate', filters.fromDate);
    if (filters.toDate) params.append('toDate', filters.toDate);
    if (filters.sortBy) params.append('sortBy', filters.sortBy);

    const response = await fetch(`${API_BASE_URL}/search/posts?${params}`);
    return response.json();
  },

  // Auth
  async login(credentials) {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(credentials),
    });
    if (!response.ok) throw new Error('Login failed');
    return response.json();
  },

  async register(userData) {
    const response = await fetch(`${API_BASE_URL}/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(userData),
    });
    if (!response.ok) throw new Error('Registration failed');
    return response.json();
  },

  // SavedPosts (Kaydedilen Gönderiler)
  async savePost(userId, postId) {
    const response = await fetch(`${API_BASE_URL}/savedposts`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ userId, postId }),
    });
    if (!response.ok) throw new Error('Failed to save post');
    return response.json();
  },

  async unsavePost(userId, postId) {
    const response = await fetch(`${API_BASE_URL}/savedposts?userId=${userId}&postId=${postId}`, {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });
    return response.status === 200 || response.status === 204;
  },

  async isSaved(userId, postId) {
    const response = await fetch(`${API_BASE_URL}/savedposts/check?userId=${userId}&postId=${postId}`);
    if (!response.ok) return false;
    return response.json();
  },

  async getSavedPosts(userId) {
    const response = await fetch(`${API_BASE_URL}/savedposts/user/${userId}`);
    if (!response.ok) throw new Error('Failed to fetch saved posts');
    return response.json();
  },
};

